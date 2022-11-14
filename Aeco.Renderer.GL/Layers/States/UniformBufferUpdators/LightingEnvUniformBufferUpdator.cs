namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;  

public class LightingEnvUniformBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    private Group<Light> _lights = new();

    private readonly Vector3 TwoVec = new Vector3(2);
    private readonly Vector3 ClusterCounts = new Vector3(
        LightingEnvParameters.ClusterCountX,
        LightingEnvParameters.ClusterCountY,
        LightingEnvParameters.ClusterCountZ);

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        _lights.Query(context);

        foreach (var id in context.Query<Camera>()) {
            ref readonly var camera = ref context.Inspect<Camera>(id);
            ref readonly var cameraMat = ref context.Inspect<CameraMatrices>(id);
            ref var buffer = ref context.Acquire<LightingEnvUniformBuffer>(id);

            if (context.Contains<Created<Camera>>(id)) {
                InitializeLightingEnv(context, ref buffer, in camera, in cameraMat);
            }
            else if (context.Contains<Modified<Camera>>(id)) {
                UpdateClusterBoundingBoxes(context, ref buffer, in camera, in cameraMat);
                UpdateClusterParameters(ref buffer, in camera);
            }
            if (_lights.Count != 0) {
                ref readonly var cameraTransformMat = ref context.Inspect<TransformMatrices>(id);
                CullLights(context, ref buffer, in camera, in cameraMat, in cameraTransformMat);
            }
        }
    }
    
    private void InitializeLightingEnv(IDataLayer<IComponent> context, ref LightingEnvUniformBuffer buffer, in Camera camera, in CameraMatrices cameraMat)
    {
        buffer.Parameters.GlobalLightIndeces = new int[LightingEnvParameters.MaximumGlobalLightCount];

        buffer.ClusterLightCounts = new int[LightingEnvParameters.ClusterCount];
        buffer.ClusterBoundingBoxes = new Rectangle[LightingEnvParameters.ClusterCount];
        UpdateClusterBoundingBoxes(context, ref buffer, in camera, in cameraMat);

        buffer.Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, buffer.Handle);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.LightingEnv, buffer.Handle);

        buffer.Pointer = GLHelper.InitializeBuffer(BufferTarget.UniformBuffer, 16 + 4 * LightingEnvParameters.MaximumGlobalLightCount);
        UpdateClusterParameters(ref buffer, in camera);
        
        // initialize texture buffer of clusters

        buffer.ClustersHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, buffer.ClustersHandle);

        buffer.ClustersPointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, 4 * LightingEnvParameters.MaximumActiveLightCount);
        buffer.ClustersTexHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.TextureBuffer, buffer.ClustersTexHandle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32i, buffer.ClustersHandle);

        // initialize texture buffer of cluster light counts

        buffer.ClusterLightCountsHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, buffer.ClusterLightCountsHandle);

        buffer.ClusterLightCountsPointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, 4 * LightingEnvParameters.ClusterCount);
        buffer.ClusterLightCountsTexHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.TextureBuffer, buffer.ClusterLightCountsTexHandle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32i, buffer.ClusterLightCountsHandle);
    }

    private void UpdateClusterBoundingBoxes(
        IDataLayer<IComponent> context, ref LightingEnvUniformBuffer buffer, in Camera camera, in CameraMatrices cameraMat)
    {
        const int countX = LightingEnvParameters.ClusterCountX;
        const int countY = LightingEnvParameters.ClusterCountY;
        const int countZ = LightingEnvParameters.ClusterCountZ;

        const float floatCountX = (float)countX;
        const float floatCountY = (float)countY;
        const float floatCountZ = (float)countZ;

        Matrix4x4.Invert(cameraMat.ProjectionRaw, out var invProj);
    
        Vector3 ScreenToView(Vector3 texCoord)
        {
            var clip = new Vector4(texCoord.X * 2 - 1, texCoord.Y * 2 - 1, texCoord.Z, 1);
            var view = Vector4.Transform(clip, invProj);
            view = view / view.W;
            return new Vector3(view.X, view.Y, view.Z);
        }

        ref var boundingBoxes = ref buffer.ClusterBoundingBoxes;
        float planeRatio = camera.FarPlaneDistance / camera.NearPlaneDistance;

        for (int x = 0; x < countX; ++x) {
            for (int y = 0; y < countY; ++y) {
                var minPoint = ScreenToView(new Vector3(x / floatCountX, y / floatCountY, -1));
                var maxPoint = ScreenToView(new Vector3((x + 1) / floatCountX, (y + 1) / floatCountY, -1));

                minPoint /= minPoint.Z;
                maxPoint /= maxPoint.Z;

                for (int z = 0; z < countZ; ++z) {
                    float tileNear = -camera.NearPlaneDistance * MathF.Pow(planeRatio, z / floatCountZ);
                    float tileFar = -camera.NearPlaneDistance * MathF.Pow(planeRatio, (z + 1) / floatCountZ);

                    var minPointNear = minPoint * tileNear;
                    var minPointFar = minPoint * tileFar;
                    var maxPointNear = maxPoint * tileNear;
                    var maxPointFar = maxPoint * tileFar;

                    ref var rect = ref boundingBoxes[x + countX * y + (countX * countY) * z];
                    rect.Min = Vector3.Min(Vector3.Min(minPointNear, minPointFar), Vector3.Min(maxPointNear, maxPointFar));
                    rect.Max = Vector3.Max(Vector3.Max(minPointNear, minPointFar), Vector3.Max(maxPointNear, maxPointFar));
                }
            }
        }
    }

    private unsafe void UpdateClusterParameters(ref LightingEnvUniformBuffer buffer, in Camera camera)
    {
        float factor = LightingEnvParameters.ClusterCountZ /
            MathF.Log2(camera.FarPlaneDistance / camera.NearPlaneDistance);
        float subtractor = MathF.Log2(camera.NearPlaneDistance) * factor;

        float* envPtr = (float*)buffer.Pointer;
        *envPtr = factor;
        *(envPtr + 1) = subtractor;

        buffer.Parameters.ClusterDepthSliceMultiplier = factor;
        buffer.Parameters.ClusterDepthSliceSubstractor = subtractor;
    }

    private unsafe void CullLights(
        IDataLayer<IComponent> context, ref LightingEnvUniformBuffer buffer,
        in Camera camera, in CameraMatrices cameraMat, in TransformMatrices cameraTransformMat)
    {
        const int countX = LightingEnvParameters.ClusterCountX;
        const int countY = LightingEnvParameters.ClusterCountY;
        const int maxClusterLightCount = LightingEnvParameters.MaximumClusterLightCount;

        var viewMat = cameraTransformMat.View;
        ref var boundingBoxes = ref buffer.ClusterBoundingBoxes;
        ref var pars = ref buffer.Parameters;

        var clusters = (int*)buffer.ClustersPointer;
        var lightCounts = buffer.ClusterLightCounts;
        Array.Clear(lightCounts);

        pars.GlobalLightCount = 0;

        foreach (var lightId in _lights) {
            ref readonly var lightData = ref context.Inspect<LightData>(lightId);
            var lightIndex = lightData.Index;

            float range = lightData.Range;
            if (range == float.PositiveInfinity) {
                if (pars.GlobalLightCount < LightingEnvParameters.MaximumGlobalLightCount) {
                    pars.GlobalLightIndeces[pars.GlobalLightCount++] = lightIndex;
                }
                continue;
            }

            var worldPos = new Vector4(context.Acquire<WorldPosition>(lightId).Value, 1);
            var viewPos = Vector4.Transform(worldPos, viewMat);

            // culled by depth
            if (viewPos.Z < -camera.FarPlaneDistance - range || viewPos.Z > -camera.NearPlaneDistance + range) {
                continue;
            }

            var rangeVec = new Vector4(range, range, 0, 0);
            var bottomLeft = Transform(viewPos - rangeVec, in cameraMat.ProjectionRaw);
            var topRight = Transform(viewPos + rangeVec, in cameraMat.ProjectionRaw);

            var screenMin = Vector2.Min(bottomLeft, topRight);
            var screenMax = Vector2.Max(bottomLeft, topRight);

            // out of screen
            if (screenMin.X > 1 || screenMax.X < -1 || screenMin.Y > 1 || screenMax.Y < -1) {
                continue;
            }

            int minX = (int)(Math.Clamp((screenMin.X + 1) / 2, 0, 1) * countX);
            int maxX = (int)MathF.Ceiling(Math.Clamp((screenMax.X + 1) / 2, 0, 1) * countX);
            int minY = (int)(Math.Clamp((screenMin.Y + 1) / 2, 0, 1) * countY);
            int maxY = (int)MathF.Ceiling(Math.Clamp((screenMax.Y + 1) / 2, 0, 1) * countY);
            int minZ = CalculateClusterDepthSlice(Math.Max(0, -viewPos.Z - range), in pars);
            int maxZ = CalculateClusterDepthSlice(-viewPos.Z + range, in pars);

            var centerPoint = new Vector3(viewPos.X, viewPos.Y, viewPos.Z);
            float rangeSq = range * range;

            for (int z = minZ; z <= maxZ; ++z) {
                for (int y = minY; y < maxY; ++y) {
                    for (int x = minX; x < maxX; ++x) {
                        int index = x + countX * y + (countX * countY) * z;
                        var lightCount = lightCounts[index];
                        if (lightCount >= maxClusterLightCount) {
                            continue;
                        }
                        if (rangeSq >= boundingBoxes[index].DistanceToPointSquared(centerPoint)) {
                            *(clusters + index * maxClusterLightCount + lightCount) = lightIndex;
                            lightCounts[index] = lightCount + 1;
                        }
                    }
                }
            }
        }

        Marshal.Copy(lightCounts, 0, buffer.ClusterLightCountsPointer, LightingEnvParameters.ClusterCount);
        *((int*)(buffer.Pointer + 8)) = pars.GlobalLightCount;
        Marshal.Copy(pars.GlobalLightIndeces, 0, buffer.Pointer + 16, pars.GlobalLightCount);
    }
    
    private static int CalculateClusterDepthSlice(float z, in LightingEnvParameters pars)
        => (int)Math.Clamp(MathF.Log2(z) * pars.ClusterDepthSliceMultiplier - pars.ClusterDepthSliceSubstractor,
                0, LightingEnvParameters.ClusterCountZ - 1);

    private static Vector2 Transform(Vector4 vec, in Matrix4x4 mat)
    {
        var res = Vector4.Transform(vec, mat);
        return new Vector2(res.X, res.Y) / res.W;
    }
}
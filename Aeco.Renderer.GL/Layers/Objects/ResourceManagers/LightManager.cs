namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class LightManager : ResourceManagerBase<Light, LightData, LightResourceBase>, IGLLoadLayer, IGLLateUpdateLayer
{
    private Stack<int> _lightIndeces = new();
    private int _maxIndex;

    public void OnLoad(IDataLayer<IComponent> context)
    {
        ref var buffer = ref context.AcquireAny<LightsBuffer>();
        buffer.Capacity = LightsBuffer.InitialCapacity;

        buffer.Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, buffer.Handle);

        buffer.Pointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, buffer.Capacity * LightParameters.MemorySize);
        buffer.TexHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.TextureBuffer, buffer.TexHandle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, buffer.Handle);

        GL.BindBuffer(BufferTarget.TextureBuffer, 0);
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
    }

    protected unsafe override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref Light light, ref LightData data, bool updating)
    {
        ref var buffer = ref context.RequireAny<LightsBuffer>();

        if (!updating) {
            if (!_lightIndeces.TryPop(out var lightIndex)) {
                lightIndex = _maxIndex++;
            }
            data.Index = lightIndex;
            if (buffer.Capacity < lightIndex) {
                var lightCount = context.GetCount<Light>();
                ResizeLightsBuffer(ref buffer, lightCount);
            }
        }

        var parsPtr = (LightParameters*)buffer.Pointer + data.Index;
        var lightRes = light.Resource;
        parsPtr->Color = lightRes.Color;

        if (lightRes is AmbientLightResource) {
            data.Category = LightCategory.Ambient;
        }
        else if (lightRes is DirectionalLightResource) {
            data.Category = LightCategory.Directional;
        }
        else if (lightRes is AttenuateLightResourceBase attLight) {
            float c = attLight.AttenuationConstant;
            float l = attLight.AttenuationLinear;
            float q = attLight.AttenuationQuadratic;

            data.Range = (-l + MathF.Sqrt(l * l - 4 * q * (c - 255 * attLight.Color.W))) / (2 * q);

            parsPtr->AttenuationConstant = c;
            parsPtr->AttenuationLinear = l;
            parsPtr->AttenuationQuadratic = q;

            switch (attLight) {
            case PointLightResource:
                data.Category = LightCategory.Point;
                break;
            case SpotLightResource spotLight:
                data.Category = LightCategory.Spot;
                parsPtr->ConeCutoffsOrAreaSize.X = MathF.Cos(spotLight.InnerConeAngle / 180f * MathF.PI);
                parsPtr->ConeCutoffsOrAreaSize.Y = MathF.Cos(spotLight.OuterConeAngle / 180f * MathF.PI);
                break;
            case AreaLightResource areaLight:
                data.Category = LightCategory.Area;
                parsPtr->ConeCutoffsOrAreaSize = areaLight.AreaSize;
                break;
            }
        }

        parsPtr->Category = (float)data.Category;
    }

    private void ResizeLightsBuffer(ref LightsBuffer buffer, int lightCount)
    {
        int requiredCapacity = buffer.Capacity;
        while (requiredCapacity < lightCount) requiredCapacity *= 2;

        var newBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, newBuffer);
        var pointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, requiredCapacity * LightParameters.MemorySize);

        GL.BindBuffer(BufferTarget.CopyReadBuffer, buffer.Handle);
        GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.TextureBuffer,
            IntPtr.Zero, IntPtr.Zero, buffer.Capacity * LightParameters.MemorySize);

        GL.DeleteTexture(buffer.TexHandle);
        GL.DeleteBuffer(buffer.Handle);

        buffer.TexHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, buffer.TexHandle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, newBuffer);

        buffer.Capacity = requiredCapacity;
        buffer.Handle = newBuffer;
        buffer.Pointer = pointer;

        GL.BindTexture(TextureTarget.TextureBuffer, 0);
        GL.BindBuffer(BufferTarget.TextureBuffer, 0);
        GL.BindBuffer(BufferTarget.CopyReadBuffer, 0);
    }

    protected override unsafe void Uninitialize(IDataLayer<IComponent> context, Guid id, in Light light, in LightData data)
    {
        ref var buffer = ref context.RequireAny<LightsBuffer>();
        ((LightParameters*)buffer.Pointer + data.Index)->Category = 0f;
        _lightIndeces.Push(data.Index);
    }
}
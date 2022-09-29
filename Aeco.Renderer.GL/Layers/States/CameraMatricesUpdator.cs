namespace Aeco.Renderer.GL;

using System.Numerics;

public class CameraMatricesUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<TransformMatricesChanged, Camera> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var camera = ref context.Acquire<Camera>(id);
            ref var matrices = ref context.Acquire<CameraMatrices>(id);

            ref readonly var pos = ref context.Inspect<Position>(id);
            ref readonly var transformMatrices = ref context.Inspect<TransformMatrices>(id);

            var size = context.Require<Window>().Current!.ClientSize;
            float aspectRatio = (float)size.X / (float)size.Y;

            var cameraDir = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, transformMatrices.World));
            var cameraUp = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, transformMatrices.World));
            matrices.View = Matrix4x4.CreateLookAt(pos, cameraDir, cameraUp);

            matrices.Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                OpenTK.Mathematics.MathHelper.DegreesToRadians(camera.FieldOfView),
                aspectRatio, camera.NearPlaneDistance, camera.FarPlaneDistance);
        }
    }
}
namespace Aeco.Renderer.GL;

using System.Numerics;

public class CameraMatricesUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<TransformMatricesDirty, Camera> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var camera = ref context.Inspect<Camera>(id);
            ref var matrices = ref context.Acquire<CameraMatrices>(id);

            var size = context.Require<Window>().Current!.ClientSize;
            float aspectRatio = (float)size.X / (float)size.Y;

            matrices.Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                OpenTK.Mathematics.MathHelper.DegreesToRadians(camera.FieldOfView),
                aspectRatio, camera.NearPlaneDistance, camera.FarPlaneDistance);
        }
    }
}
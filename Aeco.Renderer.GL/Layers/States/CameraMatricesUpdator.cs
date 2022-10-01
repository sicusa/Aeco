namespace Aeco.Renderer.GL;

using System.Numerics;

using Aeco.Reactive;

public class CameraMatricesUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<Modified<Camera>, Camera> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var camera = ref context.Inspect<Camera>(id);
            ref var matrices = ref context.Acquire<CameraMatrices>(id);

            var size = context.RequireAny<Window>().Current!.ClientSize;
            float aspectRatio = (float)size.X / (float)size.Y;

            matrices.Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                OpenTK.Mathematics.MathHelper.DegreesToRadians(camera.FieldOfView),
                aspectRatio, camera.NearPlaneDistance, camera.FarPlaneDistance);
        }
    }
}
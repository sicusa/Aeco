namespace Aeco.Renderer.GL;

using System.Numerics;

using Aeco.Reactive;

public class CameraMatricesUpdator : VirtualLayer, IGLUpdateLayer, IGLResizeLayer
{
    private Query<Modified<Camera>, Camera> _q = new();

    public void OnResize(IDataLayer<IComponent> context, int width, int height)
    {
        foreach (var id in context.Query<Camera>()) {
            UpdateCamera(context, id, width, height);
        }
    }

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        var size = context.RequireAny<Window>().Current!.ClientSize;
        foreach (var id in _q.Query(context)) {
            UpdateCamera(context, id, size.X, size.Y);
        }
    }

    private void UpdateCamera(IDataLayer<IComponent> context, Guid cameraId, int width, int height)
    {
        ref readonly var camera = ref context.Inspect<Camera>(cameraId);
        ref var matrices = ref context.Acquire<CameraMatrices>(cameraId);
        float aspectRatio = (float)width / (float)height;

        matrices.ProjectionRaw = Matrix4x4.CreatePerspectiveFieldOfView(
            OpenTK.Mathematics.MathHelper.DegreesToRadians(camera.FieldOfView),
            aspectRatio, camera.NearPlaneDistance, camera.FarPlaneDistance);
    }
}
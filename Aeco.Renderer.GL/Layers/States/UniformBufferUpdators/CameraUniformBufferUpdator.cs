namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class CameraUniformBufferUpdator : ReactiveObjectUpdatorBase<Camera>
{
    private List<Guid> _dirtyCameras = new();

    public unsafe override void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        HashSet<Guid>? dirtyIds = null; 

        foreach (var id in context.Query<Camera>()) {
            dirtyIds ??= context.AcquireAny<DirtyTransforms>().Ids;
            if (dirtyIds.Contains(id)) {
                ref var buffer = ref GetCameraBuffer(context, id, out bool exists);
                ref var pars = ref buffer.Parameters;
                pars.View = Matrix4x4.Transpose(context.UnsafeInspect<Transform>(id).View);
                pars.Proj = Matrix4x4.Transpose(context.UnsafeAcquire<CameraMatrices>(id).ProjectionRaw);
                pars.ViewProj = pars.Proj * pars.View;
                pars.Position = context.Inspect<Transform>(id).Position;
                _dirtyCameras.Add(id);
            }
        }

        base.OnUpdate(context, deltaTime);

        for (int i = 0; i != _dirtyCameras.Count; ++i) {
            ref var buffer = ref context.Acquire<CameraUniformBuffer>(_dirtyCameras[i], out bool exists);
            *((CameraParameters*)buffer.Pointer) = buffer.Parameters;
        }
        _dirtyCameras.Clear();
    }

    protected unsafe override void UpdateObject(IDataLayer<IComponent> context, Guid id)
    {
        ref readonly var camera = ref context.Inspect<Camera>(id);
        ref var buffer = ref GetCameraBuffer(context, id, out bool exists);

        ref var pars = ref buffer.Parameters;
        pars.NearPlaneDistance = camera.NearPlaneDistance;
        pars.FarPlaneDistance = camera.FarPlaneDistance;

        _dirtyCameras.Add(id);
    }

    private ref CameraUniformBuffer GetCameraBuffer(IDataLayer<IComponent> context, Guid id, out bool exists)
    {
        ref var buffer = ref context.Acquire<CameraUniformBuffer>(id, out exists);
        if (!exists) {
            buffer.Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, buffer.Handle);
            buffer.Pointer = GLHelper.InitializeBuffer(BufferTarget.UniformBuffer, CameraParameters.MemorySize);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Camera, buffer.Handle);
        }
        return ref buffer;
    }

    protected override void ReleaseObject(IDataLayer<IComponent> context, Guid id)
    {
        if (context.Remove<CameraUniformBuffer>(id, out var handle)) {
            GL.DeleteBuffer(handle.Handle);
        }
    }
}
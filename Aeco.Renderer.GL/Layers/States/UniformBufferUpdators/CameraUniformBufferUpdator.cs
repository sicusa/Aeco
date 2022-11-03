namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

public class CameraUniformBufferUpdator : ReactiveObjectUpdatorBase<Camera, TransformMatricesDirty>
{
    protected unsafe override void UpdateObject(IDataLayer<IComponent> context, Guid id)
    {
        ref readonly var camera = ref context.Inspect<Camera>(id);
        ref var buffer = ref context.Acquire<CameraUniformBuffer>(id, out bool exists);
        IntPtr pointer;

        if (!exists) {
            buffer.Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, buffer.Handle);
            pointer = GLHelper.InitializeBuffer(BufferTarget.UniformBuffer, CameraParameters.MemorySize);
            buffer.Pointer = pointer;
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Camera, buffer.Handle);
        }
        else {
            pointer = buffer.Pointer;
        }

        ref var pars = ref buffer.Parameters;
        pars.View = Matrix4x4.Transpose(context.UnsafeInspect<TransformMatrices>(id).View);
        pars.Proj = Matrix4x4.Transpose(context.UnsafeAcquire<CameraMatrices>(id).ProjectionRaw);
        pars.PrevViewProj = pars.ViewProj;
        pars.ViewProj = pars.Proj * pars.View;
        pars.Position = context.UnsafeAcquire<WorldPosition>(id).Value;
        pars.NearPlaneDistance = camera.NearPlaneDistance;
        pars.FarPlaneDistance = camera.FarPlaneDistance;

        *((CameraParameters*)pointer) = pars;
    }

    protected override void ReleaseObject(IDataLayer<IComponent> context, Guid id)
    {
        if (context.Remove<CameraUniformBuffer>(id, out var handle)) {
            GL.DeleteBuffer(handle.Handle);
        }
    }
}
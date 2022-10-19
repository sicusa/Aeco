namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class CameraUniformBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<Camera>>()) {
            if (context.Remove<CameraUniformBuffer>(id, out var handle)) {
                GL.DeleteBuffer(handle.Handle);
            }
        }
        foreach (var id in context.Query<Camera>()) {
            if (context.Contains<TransformMatricesDirty>(id) || context.Contains<Modified<Camera>>(id)) {
                DoUpdate(context, id);
            }
        }
    }

    private void DoUpdate(IDataLayer<IComponent> context, Guid id)
    {
        ref var handle = ref context.Acquire<CameraUniformBuffer>(id, out bool exists).Handle;
        if (!exists) {
            handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            GL.BufferData(BufferTarget.UniformBuffer, 64 * 3 + 16, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Camera, handle);
        }
        else {
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        }

        var view = Matrix4x4.Transpose(context.UnsafeInspect<TransformMatrices>(id).View);
        var proj = Matrix4x4.Transpose(context.UnsafeAcquire<CameraMatrices>(id).ProjectionRaw);
        var vp = proj * view;
        ref var pos = ref context.UnsafeAcquire<WorldPosition>(id).Value;

        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 64, ref view.M11);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 64, 64, ref proj.M11);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 64 * 2, 64, ref vp.M11);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 64 * 3, 12, ref pos.X);
    }
}
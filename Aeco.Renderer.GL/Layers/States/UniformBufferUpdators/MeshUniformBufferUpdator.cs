namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshUniformBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<Mesh>>()) {
            if (context.Remove<MeshUniformBuffer>(id, out var handle)) {
                GL.DeleteBuffer(handle.Handle);
            }
        }
        foreach (var id in context.Query<Mesh>()) {
            if (context.Contains<TransformMatricesDirty>(id)
                    || context.Contains<Modified<Mesh>>(id)) {
                DoUpdate(context, id);
            }
        }
    }

    private void DoUpdate(IDataLayer<IComponent> context, Guid id)
    {
        ref var handle = ref context.Acquire<MeshUniformBuffer>(id, out bool exists).Handle;
        if (!exists) {
            handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            GL.BufferData(BufferTarget.UniformBuffer, 2 * 16, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Mesh, handle);
        }
        else {
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        }

        ref var mesh = ref context.UnsafeAcquire<Mesh>(id);
        ref var boundingBox = ref mesh.Resource.BoudingBox;
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 12, ref boundingBox.Min);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16, 12, ref boundingBox.Max);
    }
}
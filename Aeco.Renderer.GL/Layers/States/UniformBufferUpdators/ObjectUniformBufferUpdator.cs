namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class ObjectUniformBufferUpdator : VirtualLayer, IGLLoadLayer, IGLUpdateLayer
{
    private Group<MeshRenderable, WorldViewDirty> _g = new();

    public void OnLoad(IDataLayer<IComponent> context)
        => _g.Refrash(context);

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<MeshRenderable>>()) {
            if (context.Remove<ObjectUniformBufferHandle>(id, out var handle)) {
                GL.DeleteBuffer(handle.Value);
            }
        }
        foreach (var id in _g.Query(context)) {
            ref var handle = ref context.Acquire<ObjectUniformBufferHandle>(id, out bool exists).Value;
            if (!exists) {
                handle = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.UniformBuffer, handle);
                GL.BufferData(BufferTarget.UniformBuffer, 64 * 2, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else {
                GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            }

            var world = Matrix4x4.Transpose(context.UnsafeAcquire<TransformMatrices>(id).WorldRaw);
            var view = Matrix4x4.Transpose(context.UnsafeAcquire<WorldView>(id).ViewRaw);

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 64, ref world.M11);
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 64, 64, ref view.M11);
        }
    }
}
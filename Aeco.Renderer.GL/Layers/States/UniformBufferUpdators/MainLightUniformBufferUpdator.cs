namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MainLightUniformBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<MainLight>>()) {
            if (context.Remove<MainLightUniformBufferHandle>(id, out var handle)) {
                GL.DeleteBuffer(handle.Value);
            }
        }
        foreach (var id in context.Query<MainLight>()) {
            if (context.Contains<TransformMatricesDirty>(id) || context.Contains<Modified<MainLight>>(id)) {
                DoUpdate(context, id);
            }
        }
    }

    private void DoUpdate(IDataLayer<IComponent> context, Guid id)
    {
        ref var handle = ref context.Acquire<MainLightUniformBufferHandle>(id, out bool exists).Value;
        if (!exists) {
            handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            GL.BufferData(BufferTarget.UniformBuffer, 16 * 2, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 4, handle);
        }
        else {
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        }

        ref var mainLight = ref context.UnsafeAcquire<MainLight>(id);
        ref var forward = ref context.UnsafeAcquire<WorldAxes>(id).Forward;

        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 16, ref forward.X);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16, 16, ref mainLight.Color.X);
    }
}
namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderableUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<MeshRenderable, TransformMatricesDirty> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Modified<MeshRenderable>>()) {
            if (!context.TryGet<MeshRenderable>(id, out var renderable))  {
                continue;
            }
            if (renderable.IsVariant) {
                UpdateVariantUniform(context, id);
            }
            else if (context.Remove<VariantUniformBufferHandle>(id, out var handle)) {
                GL.DeleteBuffer(handle.Value);
            }
        }
        foreach (var id in context.Query<Removed<MeshRenderable>>()) {
            if (context.Remove<VariantUniformBufferHandle>(id, out var handle)) {
                GL.DeleteBuffer(handle.Value);
            }
        }
        foreach (var id in _q.Query(context)) {
            ref readonly var data = ref context.Inspect<MeshRenderableData>(id);
            int index = data.InstanceIndex;
            if (index == -1) {
                UpdateVariantUniform(context, id);
                continue;
            }

            var meshData = context.Require<MeshData>(data.MeshId);
            GL.BindVertexArray(meshData.VertexArrayHandle);

            var meshState = context.Require<MeshRenderingState>(data.MeshId);
            var span = CollectionsMarshal.AsSpan(meshState.Instances);

            span[index] = new MeshInstance {
                ObjectToWorld = Matrix4x4.Transpose(context.UnsafeAcquire<TransformMatrices>(id).WorldRaw),
                WorldToObject = Matrix4x4.Transpose(context.UnsafeAcquire<WorldView>(id).ViewRaw)
            };

            int instanceBufferHandle = meshData.BufferHandles[MeshBufferType.Instance];
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBufferHandle);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero + index * MeshInstance.MemorySize, MeshInstance.MemorySize, ref span[index]);
            GL.BindVertexArray(0);
        }
    }

    private void UpdateVariantUniform(IDataLayer<IComponent> context, Guid id)
    {
        ref var handle = ref context.Acquire<VariantUniformBufferHandle>(id, out bool exists).Value;
        if (!exists) {
            handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            GL.BufferData(BufferTarget.UniformBuffer, 2 * 64 + 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
        else {
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        }

        var world = Matrix4x4.Transpose(context.UnsafeAcquire<TransformMatrices>(id).WorldRaw);
        var view = Matrix4x4.Transpose(context.UnsafeAcquire<WorldView>(id).ViewRaw);

        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 64, ref world.M11);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 64, 64, ref view.M11);

        bool isVariant = true;
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 2 * 64, 4, ref isVariant);
    }
}

namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderableUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<MeshRenderable, TransformMatricesDirty> _q = new();
    private ConcurrentDictionary<int, List<MeshInstance>> _dirtyBuffers = new();

    private List<Guid> _dirtyList = new();

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

        _dirtyList.AddRange(_q.Query(context));
        int count = _dirtyList.Count;
        if (count > 64) {
            int bundleSize = count / 4;
            Parallel.Invoke(
                () => DoUpdate(0, bundleSize, context),
                () => DoUpdate(bundleSize, 2 * bundleSize, context),
                () => DoUpdate(2 * bundleSize, 3 * bundleSize, context),
                () => DoUpdate(3 * bundleSize, count, context));
            _dirtyList.Clear();
        }
        else if (count != 0) {
            DoUpdate(0, count, context);
            _dirtyList.Clear();
        }

        foreach (var pair in _dirtyBuffers) {
            var span = CollectionsMarshal.AsSpan(pair.Value);
            GL.BindBuffer(BufferTarget.ArrayBuffer, pair.Key);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, pair.Value.Count * MeshInstance.MemorySize, ref span[0]);
            GL.BindVertexArray(0);
        }
        _dirtyBuffers.Clear();
    }

    private void DoUpdate(int start, int end, IDataLayer<IComponent> context)
    {
        for (int i = start; i != end; ++i) {
            var id = _dirtyList[i];
            var data = context.Inspect<MeshRenderableData>(id);
            int index = data.InstanceIndex;
            if (index == -1) {
                UpdateVariantUniform(context, id);
                continue;
            }

            var meshState = context.Require<MeshRenderingState>(data.MeshId);
            ref var matrices = ref context.UnsafeInspect<TransformMatrices>(id);

            meshState.Instances[index] = new MeshInstance {
                ObjectToWorld = Matrix4x4.Transpose(matrices.World),
                WorldToObject = Matrix4x4.Transpose(matrices.View)
            };

            var meshData = context.Require<MeshData>(data.MeshId);
            int instanceBufferHandle = meshData.BufferHandles[MeshBufferType.Instance];
            _dirtyBuffers.TryAdd(instanceBufferHandle, meshState.Instances);
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

        ref var matrices = ref context.UnsafeInspect<TransformMatrices>(id);
        var world = Matrix4x4.Transpose(matrices.World);
        var view = Matrix4x4.Transpose(matrices.View);

        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 64, ref world.M11);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 64, 64, ref view.M11);

        bool isVariant = true;
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 2 * 64, 4, ref isVariant);
    }
}

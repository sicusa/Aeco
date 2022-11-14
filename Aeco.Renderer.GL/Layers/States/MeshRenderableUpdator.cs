namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderableUpdator : VirtualLayer, IGLLoadLayer, IGLUpdateLayer
{
    private Query<MeshRenderable, TransformMatricesDirty> _q = new();

    private List<Guid> _dirtyList = new();
    private ConcurrentDictionary<Guid, (int, int)> _dirtyMeshes = new();
    private Action[] _actions = new Action[ParallelCount];
    private int _bundleSize;

    private const int ParallelCount = 8;

    public void OnLoad(IDataLayer<IComponent> context)
    {
        for (int i = 0; i != _actions.Length; ++i) {
            int offset = i * _bundleSize;
            if (i == _actions.Length - 1) {
                _actions[i] = () => DoUpdate(offset, _dirtyList.Count, context);
            }
            else {
                _actions[i] = () => DoUpdate(offset, offset + _bundleSize, context);
            }
        }
        OnUpdate(context, 0);
    }

    public unsafe void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Modified<MeshRenderable>>()) {
            if (!context.TryGet<MeshRenderable>(id, out var renderable))  {
                continue;
            }
            if (renderable.IsVariant) {
                UpdateVariantUniform(context, id);
            }
            else if (context.Remove<VariantUniformBuffer>(id, out var handle)) {
                GL.DeleteBuffer(handle.Handle);
            }
        }

        foreach (var id in context.Query<Removed<MeshRenderable>>()) {
            if (context.Remove<VariantUniformBuffer>(id, out var handle)) {
                GL.DeleteBuffer(handle.Handle);
            }
        }

        _dirtyList.AddRange(_q.Query(context));

        int count = _dirtyList.Count;
        if (count == 0) { return; }
        if (count > 64) {
            _bundleSize = count / _actions.Length;
            Parallel.Invoke(_actions);
        }
        else {
            DoUpdate(0, count, context);
        }

        foreach (var (meshId, range) in _dirtyMeshes) {
            var meshData = context.Require<MeshData>(meshId);
            var meshState = context.Require<MeshRenderingState>(meshId);
            var src = CollectionsMarshal.AsSpan(meshState.Instances).Slice(range.Item1, range.Item2 - range.Item1 + 1);
            var dst = new Span<MeshInstance>((void*)meshData.InstanceBufferPointer, meshState.Instances.Count);
            src.CopyTo(dst);
        }

        _dirtyMeshes.Clear();
        _dirtyList.Clear();
    }

    private unsafe void DoUpdate(int start, int end, IDataLayer<IComponent> context)
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
                ObjectToWorld = Matrix4x4.Transpose(matrices.World)
            };

            _dirtyMeshes.AddOrUpdate(data.MeshId,
                id => (index, index),
                (id, range) => (Math.Min(index, range.Item1), Math.Max(index, range.Item2)));
        }
    }

    private unsafe void UpdateVariantUniform(IDataLayer<IComponent> context, Guid id)
    {
        ref var buffer = ref context.Acquire<VariantUniformBuffer>(id, out bool exists);
        IntPtr pointer;
        if (!exists) {
            buffer.Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, buffer.Handle);
            pointer = GLHelper.InitializeBuffer(BufferTarget.UniformBuffer, MeshInstance.MemorySize + 4);
            buffer.Pointer = pointer;
        }
        else {
            pointer = buffer.Pointer;
        }

        ref var matrices = ref context.UnsafeInspect<TransformMatrices>(id);
        var world = Matrix4x4.Transpose(matrices.World);

        var ptr = (Matrix4x4*)pointer;
        *ptr = world;
        *((bool*)(ptr + 1)) = true;
    }
}

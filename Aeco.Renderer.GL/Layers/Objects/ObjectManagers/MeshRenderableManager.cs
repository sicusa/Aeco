namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL4;

public class MeshRenderableManager : ObjectManagerBase<MeshRenderable, MeshRenderableData>
{
    protected override void Initialize(IDataLayer<IComponent> context, Guid id, ref MeshRenderable renderable, ref MeshRenderableData data, bool updating)
    {
        if (updating) {
            Uninitialize(context, id, in renderable, in data);
        }

        var meshId = ResourceLibrary<MeshResource>.Reference<Mesh>(context, renderable.Mesh, id);
        data.MeshId = meshId;

        ref var state = ref context.Acquire<MeshRenderingState>(meshId);
        if (renderable.IsVariant) {
            state.VariantIds.Add(id);
            data.InstanceIndex = -1;
        }
        else {
            var instances = state.Instances;
            int index = instances.Count;
            data.InstanceIndex = index;

            ref var matrices = ref context.Acquire<TransformMatrices>(id);
            var instance = new MeshInstance {
                ObjectToWorld = Matrix4x4.Transpose(matrices.World),
                WorldToObject = Matrix4x4.Transpose(matrices.View)
            };
            instances.Add(instance);
            state.InstanceIds.Add(id);

            if (context.Contains<MeshData>(meshId)) {
                ref var meshData = ref context.Require<MeshData>(meshId);
                GL.BindVertexArray(meshData.VertexArrayHandle);

                int instanceBufferHandle = meshData.BufferHandles[MeshBufferType.Instance];
                GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBufferHandle);

                if (instances.Count <= meshData.InstanceCapacity) {
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero + index * MeshInstance.MemorySize, MeshInstance.MemorySize, ref instance);
                }
                else {
                    meshData.InstanceCapacity *= 4;

                    var newBuffer = GL.GenBuffer();
                    meshData.BufferHandles[MeshBufferType.Instance] = newBuffer;

                    GL.BindBuffer(BufferTarget.CopyWriteBuffer, newBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, meshData.InstanceCapacity * MeshInstance.MemorySize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    GL.CopyBufferSubData(BufferTarget.ArrayBuffer, BufferTarget.CopyWriteBuffer, IntPtr.Zero, IntPtr.Zero, instances.Count * MeshInstance.MemorySize);

                    GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, newBuffer);
                    GL.DeleteBuffer(instanceBufferHandle);

                    RenderHelper.SetInstancingMatrices();
                }

                GL.BindVertexArray(0);
            }
        }
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in MeshRenderable renderable, in MeshRenderableData data)
    {
        ResourceLibrary<MeshResource>.Unreference(context, data.MeshId, id);

        ref var state = ref context.Acquire<MeshRenderingState>(data.MeshId);

        int index = data.InstanceIndex;
        if (index == -1) {
            state.VariantIds.Remove(id);
            return;
        }

        var instanceIds = state.InstanceIds;
        state.Instances.RemoveAt(index);
        instanceIds.RemoveAt(index);

        if (index != instanceIds.Count) {
            for (int i = index; i != instanceIds.Count; i++) {
                var instanceId = instanceIds[i];
                context.Require<MeshRenderableData>(instanceId).InstanceIndex--;
            }
            if (context.TryGet<MeshData>(data.MeshId, out var meshData)) {
                GL.BindVertexArray(meshData.VertexArrayHandle);
                GL.BindBuffer(BufferTarget.ArrayBuffer, meshData.BufferHandles[MeshBufferType.Instance]);
                
                var span = CollectionsMarshal.AsSpan(state.Instances);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero + index * MeshInstance.MemorySize,
                    (instanceIds.Count - index) * MeshInstance.MemorySize, ref span[index]);

                GL.BindVertexArray(0);
            }
        }
    }
}
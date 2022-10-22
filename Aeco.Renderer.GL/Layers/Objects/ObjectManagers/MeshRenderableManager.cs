namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL4;

public class MeshRenderableManager : ObjectManagerBase<MeshRenderable, MeshRenderableData>
{
    protected unsafe override void Initialize(IDataLayer<IComponent> context, Guid id, ref MeshRenderable renderable, ref MeshRenderableData data, bool updating)
    {
        if (updating) {
            Uninitialize(context, id, in renderable, in data);
        }

        var meshId = ResourceLibrary<MeshResource>.Reference<Mesh>(context, renderable.Mesh, id);
        ref var state = ref context.Acquire<MeshRenderingState>(meshId);
        data.MeshId = meshId;

        if (renderable.IsVariant) {
            state.VariantIds.Add(id);
            data.InstanceIndex = -1;
        }
        else {
            var instances = state.Instances;
            int index = instances.Count;
            data.InstanceIndex = index;

            ref var matrices = ref context.Acquire<TransformMatrices>(id);
            instances.Add(new MeshInstance {
                ObjectToWorld = Matrix4x4.Transpose(matrices.World)
            });
            state.InstanceIds.Add(id);

            if (context.Contains<MeshData>(meshId)) {
                ref var meshData = ref context.Require<MeshData>(meshId);
                if (instances.Count <= meshData.InstanceCapacity) {
                    var span = CollectionsMarshal.AsSpan(instances);
                    fixed (MeshInstance* ptr = span) {
                        int offset = index * MeshInstance.MemorySize;
                        System.Buffer.MemoryCopy(ptr + index, (void*)(meshData.InstanceBufferPointer + offset),
                            MeshInstance.MemorySize, MeshInstance.MemorySize);
                    }
                }
                else {
                    meshData.InstanceCapacity *= 4;

                    int instanceBufferHandle = meshData.BufferHandles[MeshBufferType.Instance];
                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBufferHandle);

                    var newBuffer = GL.GenBuffer();
                    meshData.BufferHandles[MeshBufferType.Instance] = newBuffer;

                    MeshManager.InitializeInstanceBuffer(BufferTarget.CopyWriteBuffer, newBuffer, ref meshData);
                    GL.CopyBufferSubData(BufferTarget.ArrayBuffer, BufferTarget.CopyWriteBuffer, IntPtr.Zero, IntPtr.Zero, instances.Count * MeshInstance.MemorySize);

                    GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
                    GL.DeleteBuffer(instanceBufferHandle);

                    GL.BindVertexArray(meshData.VertexArrayHandle);
                    MeshManager.InitializeInstanceCulling(ref meshData);
                    GL.BindVertexArray(0);
                }
            }
        }
    }

    protected unsafe override void Uninitialize(IDataLayer<IComponent> context, Guid id, in MeshRenderable renderable, in MeshRenderableData data)
    {
        ResourceLibrary<MeshResource>.Unreference(context, data.MeshId, id);

        ref var state = ref context.Acquire<MeshRenderingState>(data.MeshId);

        int index = data.InstanceIndex;
        if (index == -1) {
            state.VariantIds.Remove(id);
            return;
        }

        var instances = state.Instances;
        var instanceIds = state.InstanceIds;

        instances.RemoveAt(index);
        instanceIds.RemoveAt(index);

        if (index != instanceIds.Count) {
            for (int i = index; i != instanceIds.Count; i++) {
                var instanceId = instanceIds[i];
                context.Require<MeshRenderableData>(instanceId).InstanceIndex--;
            }
            if (context.TryGet<MeshData>(data.MeshId, out var meshData)) {
                var span = CollectionsMarshal.AsSpan(instances);
                fixed (MeshInstance* ptr = span) {
                    int offset = index * MeshInstance.MemorySize;
                    int length = (instanceIds.Count - index) * MeshInstance.MemorySize;
                    System.Buffer.MemoryCopy(ptr + index, (void*)(meshData.InstanceBufferPointer + offset), length, length);
                }
            }
        }
    }
}
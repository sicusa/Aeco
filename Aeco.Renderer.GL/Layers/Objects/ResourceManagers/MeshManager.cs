namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using System.Runtime.InteropServices;

public class MeshManager : ResourceManagerBase<Mesh, MeshData, MeshResource>
{
    protected unsafe override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref Mesh mesh, ref MeshData data, bool updating)
    {
        if (updating) {
            Uninitialize(context, id, in mesh, in data);
        }

        var resource = mesh.Resource;
        var material = resource.Material ?? MaterialResource.Default;
        data.MaterialId = ResourceLibrary<MaterialResource>.Reference<Material>(context, material, id);
        data.IndexCount = resource.Indeces!.Length;

        var buffers = data.BufferHandles;
        data.VertexArrayHandle = GL.GenVertexArray();
        data.CulledQueryHandle = GL.GenQuery();

        GL.BindVertexArray(data.VertexArrayHandle);
        GL.GenBuffers(buffers.Length, buffers.Raw);

        if (resource.Vertices != null) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[MeshBufferType.Vertex]);
            GL.BufferData(BufferTarget.ArrayBuffer, resource.Vertices.Length * 3 * sizeof(float), resource.Vertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        }
        if (resource.TexCoords != null) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[MeshBufferType.TexCoord]);
            GL.BufferData(BufferTarget.ArrayBuffer, resource.TexCoords.Length * 3 * sizeof(float), resource.TexCoords, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        }
        if (resource.Normals != null) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[MeshBufferType.Normal]);
            GL.BufferData(BufferTarget.ArrayBuffer, resource.Normals.Length * 3 * sizeof(float), resource.Normals, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
        }
        if (resource.Tangents != null) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[MeshBufferType.Tangent]);
            GL.BufferData(BufferTarget.ArrayBuffer, resource.Tangents.Length * 3 * sizeof(float), resource.Tangents, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
        }

        if (resource.Indeces != null) {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[MeshBufferType.Index]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, resource.Indeces.Length * sizeof(int), resource.Indeces, BufferUsageHint.StaticDraw);
        }

        // instance

        if (context.TryGet<MeshRenderingState>(id, out var state) && state.Instances.Count != 0) {
            data.InstanceCapacity = Math.Max(data.InstanceCapacity, state.Instances.Count);
            InitializeInstanceBuffer(ref data);

            var span = CollectionsMarshal.AsSpan(state.Instances);
            fixed (MeshInstance* ptr = span) {
                var length = span.Length * MeshInstance.MemorySize;
                System.Buffer.MemoryCopy(ptr, (void*)data.InstanceBufferPointer, length, length);
            }
        }
        else {
            InitializeInstanceBuffer(ref data);
        }

        data.CullingVertexArrayHandle = GL.GenVertexArray();
        InitializeInstanceCulling(ref data);

        GL.BindVertexArray(0);
    }

    public static void InitializeInstanceBuffer(ref MeshData data)
        => InitializeInstanceBuffer(
            BufferTarget.ArrayBuffer, data.BufferHandles[MeshBufferType.Instance], ref data);

    public static void InitializeInstanceBuffer(BufferTarget target, int handle, ref MeshData data)
    {
        GL.BindBuffer(target, handle);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            GL.BufferData(target, data.InstanceCapacity * MeshInstance.MemorySize, IntPtr.Zero, BufferUsageHint.StaticDraw);
        }
        else {
            GL.BufferStorage(target,
                data.InstanceCapacity * MeshInstance.MemorySize, IntPtr.Zero,
                BufferStorageFlags.MapWriteBit | BufferStorageFlags.DynamicStorageBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        }
        data.InstanceBufferPointer = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
    }

    public static void InitializeInstanceCulling(ref MeshData data)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, data.BufferHandles[MeshBufferType.CulledInstance]);
        GL.BufferData(BufferTarget.ArrayBuffer, data.InstanceCapacity * MeshInstance.MemorySize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        RenderHelper.EnableMatrix4x4Attributes(4, 1);

        GL.BindVertexArray(data.CullingVertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, data.BufferHandles[MeshBufferType.Instance]);
        RenderHelper.EnableMatrix4x4Attributes(0);
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in Mesh mesh, in MeshData data)
    {
        GL.DeleteBuffers(data.BufferHandles.Length, data.BufferHandles.Raw);
        GL.DeleteVertexArray(data.VertexArrayHandle);
        GL.DeleteQuery(data.CulledQueryHandle);
        ResourceLibrary<MaterialResource>.Unreference(context, data.MaterialId, id);
    }
}
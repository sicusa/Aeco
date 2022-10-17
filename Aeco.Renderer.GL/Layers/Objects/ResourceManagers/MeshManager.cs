namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using System.Runtime.InteropServices;

public class MeshManager : ResourceManagerBase<Mesh, MeshData, MeshResource>
{
    protected override void Initialize(
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

        GL.BindVertexArray(data.VertexArrayHandle);
        GL.GenBuffers(buffers.Count, buffers.Raw);

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

        GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[MeshBufferType.Instance]);

        if (context.TryGet<MeshRenderingState>(id, out var state) && state.Instances.Count != 0) {
            data.InstanceCapacity = Math.Max(data.InstanceCapacity, state.Instances.Count);
            GL.BufferData(BufferTarget.ArrayBuffer, data.InstanceCapacity * MeshInstance.MemorySize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            var span = CollectionsMarshal.AsSpan(state.Instances);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, state.Instances.Count * MeshInstance.MemorySize, ref span[0]);
        }
        else {
            GL.BufferData(BufferTarget.ArrayBuffer, data.InstanceCapacity * MeshInstance.MemorySize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        RenderHelper.SetInstancingMatrices();
        GL.BindVertexArray(0);
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in Mesh mesh, in MeshData data)
    {
        GL.DeleteBuffers(data.BufferHandles.Count, data.BufferHandles.Raw);
        GL.DeleteVertexArray(data.VertexArrayHandle);
        ResourceLibrary<MaterialResource>.Unreference(context, data.MaterialId, id);
    }
}
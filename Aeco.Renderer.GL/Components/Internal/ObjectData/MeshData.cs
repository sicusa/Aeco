namespace Aeco.Renderer.GL;

public enum MeshBufferType
{
    Index,
    Vertex,
    TexCoord,
    Normal,
    Tangent,
    MVPMatrix,
    WorldMatrix
}

public struct MeshData : IGLObject
{
    public int IndexCount = 0;
    public int VertexArrayHandle = 0;
    public readonly EnumArray<MeshBufferType, int> BufferHandles = new();
    public Guid MaterialId = Guid.Empty;

    public MeshData() {}

    public void Dispose() { this = new(); }
}
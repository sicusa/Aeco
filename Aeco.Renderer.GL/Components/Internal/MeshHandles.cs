namespace Aeco.Renderer.GL;

public struct MeshHandles : IGLObject
{
    public int VertexArray;
    public int VertexBuffer;
    public int ElementBuffer;

    public void Dispose() { this = default; }
}
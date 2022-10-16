namespace Aeco.Renderer.GL;

public struct MeshRenderableData : IGLObject
{
    public Guid MeshId;

    public void Dispose() { this = new(); }
}
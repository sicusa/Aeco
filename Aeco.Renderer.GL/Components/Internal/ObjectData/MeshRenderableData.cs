namespace Aeco.Renderer.GL;

public struct MeshRenderableData : IGLObject
{
    public Guid MeshId;
    public int InstanceIndex;

    public void Dispose() { this = new(); }
}
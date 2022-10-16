namespace Aeco.Renderer.GL;

public struct MeshRenderableData : IGLObject
{
    public Guid MeshId;
    public int InstanceId;

    public void Dispose() { this = new(); }
}
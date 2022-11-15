namespace Aeco.Renderer.GL;

public struct DirtyTransforms : IGLObject
{
    public readonly HashSet<Guid> Ids = new();

    public DirtyTransforms() {}

    public void Dispose() => this = new();
}
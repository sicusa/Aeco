namespace Aeco.Renderer.GL;

public struct DirtyTransforms : IGLSingleton
{
    public readonly HashSet<Guid> Ids = new();

    public DirtyTransforms() {}
}
namespace Aeco.Renderer.GL;

using System.Collections.Immutable;

public struct RenderingList : IGLObject
{
    public ImmutableArray<Guid> Ids = ImmutableArray<Guid>.Empty;

    public RenderingList() {}

    public void Dispose() { this = new(); }
}
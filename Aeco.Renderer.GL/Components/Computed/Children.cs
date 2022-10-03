namespace Aeco.Renderer.GL;

using System.Collections.Immutable;

public struct Children : IGLObject
{
    public ImmutableHashSet<Guid> Ids { get; internal set; }
        = ImmutableHashSet<Guid>.Empty;

    public Children() {}

    public void Dispose() { this = new(); }
}
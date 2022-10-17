namespace Aeco.Renderer.GL;

using System.Collections.Immutable;

public struct Children : IGLObject
{
    public ImmutableList<Guid> Ids { get; internal set; }
        = ImmutableList<Guid>.Empty;

    public Children() {}

    public void Dispose() { this = new(); }
}
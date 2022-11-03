namespace Aeco.Renderer.GL;

using System.Collections.Immutable;
using System.Runtime.Serialization;

[DataContract]
public struct ResourceReferencers : IGLObject
{
    [DataMember] public ImmutableHashSet<Guid> Ids = ImmutableHashSet<Guid>.Empty;

    public ResourceReferencers() {}

    public void Dispose() => this = new();
}
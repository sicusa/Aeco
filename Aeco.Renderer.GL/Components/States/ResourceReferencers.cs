namespace Aeco.Renderer.GL;

using System.Collections.Immutable;
using System.Runtime.Serialization;

[DataContract]
public struct ResourceReferencers : IGLObject
{
    [DataMember] public ImmutableList<Guid> Ids = ImmutableList<Guid>.Empty;

    public ResourceReferencers() {}

    public void Dispose() { this = new(); }
}
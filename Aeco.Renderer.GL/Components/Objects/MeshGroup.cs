namespace Aeco.Renderer.GL;

using System.Collections.Immutable;
using System.Runtime.Serialization;

[DataContract]
public struct MeshGroup : IGLObject
{
    [DataMember] public ImmutableArray<Guid> MeshIds =
        ImmutableArray<Guid>.Empty;

    public MeshGroup() {}

    public void Dispose() { this = new(); }
}
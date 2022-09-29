namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct MeshRenderable : IGLReactiveObject
{
    [DataMember] public Guid Mesh;
    [DataMember] public Guid[] Materials;

    public void Dispose() { this = new(); }
}
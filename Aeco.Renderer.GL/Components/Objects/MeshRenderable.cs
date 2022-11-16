namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct MeshRenderable : IGLReactiveObject
{
    public MeshResource Mesh;

    [DataMember] public bool IsVariant;
}
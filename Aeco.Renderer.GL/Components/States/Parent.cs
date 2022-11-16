namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Parent : IGLReactiveObject
{
    [DataMember] public Guid Id = GLRenderer.RootId;

    public Parent() {}
}
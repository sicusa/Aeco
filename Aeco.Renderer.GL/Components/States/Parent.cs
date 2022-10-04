namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Parent : IGLReactiveObject, IDisposable
{
    [DataMember] public Guid Id = GLRenderer.RootId;

    public Parent() {}

    public void Dispose() { this = new(); }
}
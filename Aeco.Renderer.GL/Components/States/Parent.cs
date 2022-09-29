namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Parent : IGLReactiveObject, IDisposable
{
    [DataMember] public Guid Id;

    public void Dispose() { this = new(); }
}
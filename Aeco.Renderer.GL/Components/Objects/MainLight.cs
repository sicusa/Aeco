namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct MainLight : IGLReactiveObject
{
    [DataMember]
    public Vector4 Color = Vector4.One;

    public MainLight() {}

    public void Dispose() { this = new(); }
}
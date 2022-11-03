namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Camera : IGLReactiveObject
{
    [DataMember] public float FieldOfView = 60f;
    [DataMember] public float NearPlaneDistance = 0.01f;
    [DataMember] public float FarPlaneDistance = 200f;

    public Camera() {}

    public void Dispose() => this = new();
}
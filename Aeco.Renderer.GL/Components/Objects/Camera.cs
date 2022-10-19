namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Camera : IGLReactiveObject
{
    public float FieldOfView = 60f;
    public float NearPlaneDistance = 0.01f;
    public float FarPlaneDistance = 200f;

    public Camera() {}

    public void Dispose() { this = new(); }
}
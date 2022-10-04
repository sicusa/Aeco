namespace Aeco.Renderer.GL;

using System.Numerics;

public struct WorldView : IGLReactiveObject
{
    public Matrix4x4 View => ViewRaw;
    internal Matrix4x4 ViewRaw;

    public Vector3 Forward;
    public Vector3 Up;
    public Vector3 Right;

    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

using System.Numerics;

public struct WorldView : IGLObject
{
    public Matrix4x4 View;
    public Vector3 Forward;
    public Vector3 Up;
    public Vector3 Right;

    public void Dispose() { this = default; }
}
namespace Aeco.Renderer.GL;

using System.Numerics;

public struct AppliedWorldVectors : IGLObject
{
    public Vector3 Forward;
    public Vector3 Up;
    public Vector3 Right;

    public void Dispose() { this = new(); }
}
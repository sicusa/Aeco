namespace Aeco.Renderer.GL;

using System.Numerics;

public struct TransformMatrices : IGLObject, IDisposable
{
    public Matrix4x4 World = Matrix4x4.Identity;
    public Matrix4x4 Combined = Matrix4x4.Identity;
    public Matrix4x4 Translation = Matrix4x4.Identity;
    public Matrix4x4 Rotation = Matrix4x4.Identity;
    public Matrix4x4 Scale = Matrix4x4.Identity;

    public TransformMatrices() {}

    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

using System.Numerics;

public struct TransformMatrices : IGLObject
{
    public Matrix4x4 World => WorldRaw;
    internal Matrix4x4 WorldRaw = Matrix4x4.Identity;

    public Matrix4x4 Combined { get; internal set; }
        = Matrix4x4.Identity;
    public Matrix4x4 Translation { get; internal set; }
        = Matrix4x4.Identity;
    public Matrix4x4 Rotation { get; internal set; }
        = Matrix4x4.Identity;
    public Matrix4x4 Scale { get; internal set; }
        = Matrix4x4.Identity;

    public TransformMatrices() {}

    public void Dispose() { this = new(); }
}
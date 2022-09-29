namespace Aeco.Renderer.GL;

using System.Numerics;

public struct CameraMatrices : IGLObject, IDisposable
{
    public Matrix4x4 View = Matrix4x4.Identity;
    public Matrix4x4 Projection = Matrix4x4.Identity;

    public CameraMatrices() {}

    public void Dispose() { this = default; }
}
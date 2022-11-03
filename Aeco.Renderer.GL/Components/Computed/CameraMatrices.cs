namespace Aeco.Renderer.GL;

using System.Numerics;

public struct CameraMatrices : IGLObject, IDisposable
{
    public Matrix4x4 Projection => ProjectionRaw;
    internal Matrix4x4 ProjectionRaw = Matrix4x4.Identity;

    public CameraMatrices() {}

    public void Dispose() => this = new();
}
namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CameraParameters
{
    public const int MemorySize = 3 * 64 + 12 + 2 * 4;

    public Matrix4x4 View;
    public Matrix4x4 Projection;
    public Matrix4x4 ViewProjection;
    public Vector3 Position;
    public float NearPlaneDistance;
    public float FarPlaneDistance;
}

public struct CameraUniformBuffer : IGLObject
{
    public int Handle;
    public IntPtr Pointer;
    public CameraParameters Parameters;

    public void Dispose() { this = new(); }
}
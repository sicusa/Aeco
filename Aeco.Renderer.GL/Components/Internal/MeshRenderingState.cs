namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeshInstance
{
    public const int MemorySize = 16 * sizeof(float);
    public Matrix4x4 ObjectToWorld;
}

public struct MeshRenderingState : IGLObject
{
    public readonly List<MeshInstance> Instances = new();
    public readonly List<Guid> InstanceIds = new();
    public readonly List<Guid> VariantIds = new();

    public MeshRenderingState() {}

    public void Dispose() { this = new(); }
}
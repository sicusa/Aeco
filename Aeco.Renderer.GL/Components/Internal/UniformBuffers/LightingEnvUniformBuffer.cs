namespace Aeco.Renderer.GL;

public struct LightingEnvUniformBuffer : IGLObject
{
    public const int ClusterCountX = 16;
    public const int ClusterCountY = 9;
    public const int ClusterCountZ = 24;
    public const int ClusterCount = ClusterCountX * ClusterCountY * ClusterCountZ;
    public const int ClusterMaximumLightCount = 32;
    public const int MaximumActiveLightCount = ClusterCount * ClusterMaximumLightCount;

    public const int InitialCapacity = 32;

    public int Handle;
    public IntPtr Pointer;

    public int LightsHandle;
    public int LightsTexHandle;
    public IntPtr LightsPointer;
    public int Capacity;

    public void Dispose() => this = new();
}
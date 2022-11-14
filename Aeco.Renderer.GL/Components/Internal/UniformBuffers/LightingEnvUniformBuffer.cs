namespace Aeco.Renderer.GL;

public struct LightingEnvUniformBuffer : IGLObject
{
    public const int ClusterCountX = 16;
    public const int ClusterCountY = 9;
    public const int ClusterCountZ = 24;
    public const int ClusterCount = ClusterCountX * ClusterCountY * ClusterCountZ;
    public const int MaximumClusterLightCount = 64;
    public const int MaximumActiveLightCount = ClusterCount * MaximumClusterLightCount;

    public int Handle;
    public IntPtr Pointer;

    public int ClustersHandle;
    public int ClustersTexHandle;
    public IntPtr ClustersPointer;

    public int ClusterLightCountsHandle;
    public int ClusterLightCountsTexHandle;
    public IntPtr ClusterLightCountsPointer;

    public float ClusterDepthSliceMultiplier;
    public float ClusterDepthSliceSubstractor;

    public int[] ClusterLightCounts;
    public Rectangle[] ClusterBoundingBoxes;

    public void Dispose() => this = new();
}
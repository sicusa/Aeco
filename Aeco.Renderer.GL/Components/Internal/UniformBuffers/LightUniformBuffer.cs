namespace Aeco.Renderer.GL;

public struct LightUniformBuffer : IGLObject
{
    public const int InitialCapacity = 32;
    public const int TileVerticalCount = 8;
    public const int TileHorizontalCount = 8;
    public const int TileTotalCount = TileVerticalCount * TileHorizontalCount;
    public const int TileMaximumLightCount = 8;

    public int Handle;
    public IntPtr Pointer;

    public int LightsHandle;
    public int LightsTexHandle;
    public IntPtr LightsPointer;
    public int Capacity;

    public void Dispose() => this = new();
}
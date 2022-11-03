namespace Aeco.Renderer.GL;

public struct LightUniformBuffer : IGLObject
{
    public const int InitialCapacity = 512;
    public const int TileVerticalCount = 8;
    public const int TileHorizontalCount = 8;
    public const int TileTotalCount = TileVerticalCount * TileHorizontalCount;
    public const int TileMaximumLightCount = 8;

    public int Handle;
    public int Capacity;
    public IntPtr Pointer;

    public void Dispose() => this = new();
}
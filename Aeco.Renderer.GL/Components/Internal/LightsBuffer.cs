namespace Aeco.Renderer.GL;

public struct LightsBuffer : IGLSingleton
{
    public const int InitialCapacity = 512;

    public int Capacity;

    public int Handle;
    public int TexHandle;
    public IntPtr Pointer;

    public LightParameters[] Parameters;
}
namespace Aeco.Renderer.GL;

public struct LightsBuffer : IGLSingleton
{
    public const int InitialCapacity = 32;

    public int Capacity;

    public int Handle;
    public int TexHandle;
    public IntPtr Pointer;

    public LightParameters[] Parameters;

    public void Dispose() => this = new();
}
namespace Aeco.Renderer.GL;

public struct MainLightUniformBufferHandle : IGLObject
{
    public int Value = -1;

    public MainLightUniformBufferHandle() {}

    public void Dispose() { this = new(); }
}
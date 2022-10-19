namespace Aeco.Renderer.GL;

public struct MainLightUniformBuffer : IGLObject
{
    public int Handle = -1;

    public MainLightUniformBuffer() {}

    public void Dispose() { this = new(); }
}
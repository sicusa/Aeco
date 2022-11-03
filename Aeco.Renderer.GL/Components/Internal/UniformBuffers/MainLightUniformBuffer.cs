namespace Aeco.Renderer.GL;

public struct MainLightUniformBuffer : IGLObject
{
    public int Handle;

    public void Dispose() => this = new();
}
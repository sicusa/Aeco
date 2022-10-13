namespace Aeco.Renderer.GL;

public struct TextureData : IGLObject
{
    public int Handle;

    public void Dispose() { this = new(); }
}
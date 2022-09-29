namespace Aeco.Renderer.GL;

public struct TextureHandle : IGLObject
{
    public int Value;

    public void Dispose() { this = new(); }
}
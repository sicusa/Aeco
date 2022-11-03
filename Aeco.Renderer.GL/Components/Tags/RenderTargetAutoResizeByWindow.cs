namespace Aeco.Renderer.GL;

public struct RenderTargetAutoResizeByWindow : IGLObject
{
    public void Dispose() => this = new();
}
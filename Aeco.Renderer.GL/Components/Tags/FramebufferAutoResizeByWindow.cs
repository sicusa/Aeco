namespace Aeco.Renderer.GL;

public struct FramebufferAutoResizeByWindow : IGLObject
{
    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

public enum ScreenBuffer
{
    Color,
    TransparencyAccum,
    TransparencyAlpha,
    MaxDepth,
    MinDepth
}

public struct RenderTargetDebug : IGLObject
{
    public ScreenBuffer VisibleBuffer;

    public void Dispose() { this = new(); }
}
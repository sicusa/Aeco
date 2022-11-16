namespace Aeco.Renderer.GL;

public enum DisplayMode
{
    Color,
    TransparencyAccum,
    TransparencyAlpha,
    Depth,
    Clusters
}

public struct RenderTargetDebug : IGLObject
{
    public DisplayMode DisplayMode;
}
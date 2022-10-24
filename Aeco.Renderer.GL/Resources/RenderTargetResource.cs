namespace Aeco.Renderer.GL;

public record RenderTargetResource : IGLResource
{
    public static readonly RenderTargetResource Default = new() {
        AutoResizeByWindow = true
    };

    public int Width;
    public int Height;
    public bool AutoResizeByWindow = false;
}
namespace Aeco.Renderer.GL;

public record FramebufferResource : IGLResource
{
    public static readonly FramebufferResource Default = new() {
        AutoResizeByWindow = true
    };

    public int Width;
    public int Height;
    public bool AutoResizeByWindow = false;
}
namespace Aeco.Renderer.GL;

public class FramebufferResource : IGLResource
{
    public static readonly FramebufferResource Default = new() {
        AutoResizeByWindow = true
    };

    public int Width;
    public int Height;
    public bool AutoResizeByWindow = false;
}
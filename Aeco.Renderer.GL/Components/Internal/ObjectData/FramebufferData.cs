namespace Aeco.Renderer.GL;

public struct FramebufferData : IGLObject
{
    public int Handle;
    public int ColorTextureHandle;
    public int TransparencyAccumTextureHandle;
    public int TransparencyAlphaTextureHandle;
    public int DepthTextureHandle;
    public int UniformBufferHandle;

    public int Width;
    public int Height;

    public void Dispose() { this = new(); }
}
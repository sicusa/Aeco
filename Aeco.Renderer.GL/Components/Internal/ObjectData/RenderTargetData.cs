namespace Aeco.Renderer.GL;

public struct RenderTargetData : IGLObject
{
    public int UniformBufferHandle;
    public int Width;
    public int Height;

    public int ColorFramebufferHandle;
    public int ColorTextureHandle;
    public int MaxDepthTextureHandle;
    public int MinDepthTextureHandle;

    public int TransparencyFramebufferHandle;
    public int TransparencyAccumTextureHandle;
    public int TransparencyAlphaTextureHandle;

    public void Dispose() { this = new(); }
}
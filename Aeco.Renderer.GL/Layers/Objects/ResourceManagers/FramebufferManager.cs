namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class FramebufferManager : ResourceManagerBase<Framebuffer, FramebufferData, FramebufferResource>, IGLResizeLayer
{
    private int _windowWidth;
    private int _windowHeight;

    public void OnResize(IDataLayer<IComponent> context, int width, int height)
    {
        foreach (var id in context.Query<FramebufferAutoResizeByWindow>()) {
            ref var framebuffer = ref context.UnsafeInspect<Framebuffer>(id);
            ref var data = ref context.Require<FramebufferData>(id);
            DeleteTextures(in data);
            UpdateData(context, id, ref framebuffer, ref data, width, height);
        }
        _windowWidth = width;
        _windowHeight = height;
    }

    protected override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref Framebuffer framebuffer, ref FramebufferData data, bool updating)
    {
        var resource = framebuffer.Resource;
        int width = resource.Width;
        int height = resource.Height;

        if (updating) {
            DeleteTextures(in data);
        }
        else {
            data.Handle = GL.GenFramebuffer();
            data.UniformBufferHandle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.UniformBuffer, data.UniformBufferHandle);
            GL.BufferData(BufferTarget.UniformBuffer, 8, IntPtr.Zero, BufferUsageHint.StaticDraw);

            if (resource.AutoResizeByWindow) {
                context.Acquire<FramebufferAutoResizeByWindow>(id);
                width = _windowWidth;
                height = _windowHeight;
            }
        }
        UpdateData(context, id, ref framebuffer, ref data, width, height);
    }

    private void UpdateData(
        IDataLayer<IComponent> context, Guid id, ref Framebuffer framebuffer, ref FramebufferData data, int width, int height)
    {
        data.Width = width;
        data.Height = height;
        GL.BindBuffer(BufferTarget.UniformBuffer, data.UniformBufferHandle);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 4, ref data.Width);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 4, 4, ref data.Height);

        data.ColorTextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, data.ColorTextureHandle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        data.DepthTextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, data.DepthTextureHandle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, width, height, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, data.Handle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, data.ColorTextureHandle, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, data.DepthTextureHandle, 0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in Framebuffer framebuffer, in FramebufferData data)
    {
        DeleteTextures(in data);
        context.Remove<FramebufferAutoResizeByWindow>(id);

        GL.DeleteFramebuffer(data.Handle);
        GL.DeleteBuffer(data.UniformBufferHandle);
    }

    private void DeleteTextures(in FramebufferData data)
    {
        GL.DeleteTexture(data.ColorTextureHandle);
        GL.DeleteTexture(data.DepthTextureHandle);
    }
}
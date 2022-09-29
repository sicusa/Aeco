namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class TextureUninitializer : UninitializerBase<Texture, TextureHandle>
{
    protected override void Uninitialize(in TextureHandle handle)
    {
        GL.DeleteTexture(handle.Value);
    }
}
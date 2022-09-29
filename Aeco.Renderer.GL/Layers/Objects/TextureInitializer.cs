namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

using Aeco.Reactive;

public class TextureInitializer : VirtualLayer, IGLUpdateLayer
{
    private Query<Modified<Texture>, Texture> _q = new();

    public TextureInitializer()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
    }

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var texture = ref context.Inspect<Texture>(id);

            var stream = texture.Stream;
            if (stream == null) {
                Console.WriteLine($"Texture {id} failed to initialize: texture stream not set.");
                continue;
            }
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            ref var handle = ref context.Acquire<TextureHandle>(id, out bool exists);
            if (!exists) {
                handle.Value = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, handle.Value);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)texture.WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)texture.WrapT);

            if (texture.BorderColor != null) {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, texture.BorderColor);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)texture.MinFitler);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)texture.MaxFitler);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0, PixelFormat.Rgba,
                PixelType.UnsignedByte, image.Data);

            if (texture.MipmapEnabled) {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            Console.WriteLine($"Texture {id} initialized.");
        }
    }
}
namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public enum TextureType
{
    Diffuse,
    Specular,
    Ambient,
    Emissive,
    Height,
    Normal,
    Shininess,
    Opacity,
    Displacement,
    LightMap,
    Reflection,

    Unknown
}

public record TextureResource : IGLResource
{
    public static readonly TextureResource None = new(ImageResource.Hint);
    public static readonly TextureResource White = new(ImageResource.White);

    public ImageResource Image;
    public TextureType TextureType = TextureType.Unknown;

    public TextureWrapMode WrapU = TextureWrapMode.Repeat;
    public TextureWrapMode WrapV = TextureWrapMode.Repeat;

    public TextureMinFilter MinFitler = TextureMinFilter.LinearMipmapLinear;
    public TextureMagFilter MaxFitler = TextureMagFilter.Linear;

    public float[]? BorderColor = null;
    public bool MipmapEnabled = true;

    public TextureResource(ImageResource image)
    {
        Image = image;
    }
}
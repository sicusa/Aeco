namespace Aeco.Renderer.GL;

using System.Numerics;

public class MaterialResource : IGLResource
{
    public static readonly MaterialResource Default = new();

    public Vector4 DiffuseColor = Vector4.One;
    public Vector4 SpecularColor;
    public Vector4 AmbientColor;
    public Vector4 EmissiveColor;
    public float Shininess;
    public float ShininessStrength;
    public float Reflectivity;
    public float Opacity = 1f;

    public readonly EnumArray<TextureType, TextureResource?> Textures = new();
}
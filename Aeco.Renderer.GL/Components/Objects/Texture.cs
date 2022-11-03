namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Texture : IGLResourceObject<TextureResource>
{
    public TextureResource Resource { get; set; }

    public void Dispose() => this = new();
}
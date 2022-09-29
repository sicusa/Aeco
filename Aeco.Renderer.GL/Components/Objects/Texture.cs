namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

using OpenTK.Graphics.OpenGL4;

[DataContract]
public struct Texture : IGLReactiveObject
{
    [DataMember] public Stream? Stream = null;

    [DataMember] public TextureWrapMode WrapS = TextureWrapMode.Repeat;
    [DataMember] public TextureWrapMode WrapT = TextureWrapMode.Repeat;
    [DataMember] public float[]? BorderColor = null;

    [DataMember] public bool MipmapEnabled = true;
    [DataMember] public TextureMinFilter MinFitler = TextureMinFilter.LinearMipmapLinear;
    [DataMember] public TextureMagFilter MaxFitler = TextureMagFilter.Linear;

    public Texture() { }

    public void Dispose() { this = new(); }
}
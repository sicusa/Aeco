namespace Aeco.Renderer.GL;

public struct MaterialData : IGLObject
{
    public int Handle;
    public Guid ShaderProgramId;
    public EnumArray<TextureType, Guid?> Textures;

    public void Dispose() { this = new(); }
}
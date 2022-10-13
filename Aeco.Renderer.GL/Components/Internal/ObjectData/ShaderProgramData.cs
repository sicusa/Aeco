namespace Aeco.Renderer.GL;

public struct UniformLocations
{
    public EnumArray<TextureType,  int> Textures;

    public int CameraBlock;
    public int ObjectBlock;
    public int MaterialBlock;
    public int MainLightBlock;
}

public struct ShaderProgramData : IGLObject
{
    public int Handle;
    public UniformLocations UniformLocations;

    public void Dispose() { this = new(); }
}
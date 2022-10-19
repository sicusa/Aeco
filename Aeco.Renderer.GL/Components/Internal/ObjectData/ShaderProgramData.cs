namespace Aeco.Renderer.GL;

public struct UniformLocations
{
    public EnumArray<TextureType, int> Textures;

    public int CameraBlock;
    public int MainLightBlock;
    public int MaterialBlock;
    public int ObjectBlock;
    public int MeshBlock;
}

public enum UniformBlockBinding
{
    Camera = 1,
    MainLight,
    Material,
    Mesh,
    Object
}

public struct ShaderProgramData : IGLObject
{
    public int Handle;
    public UniformLocations UniformLocations;

    public void Dispose() { this = new(); }
}
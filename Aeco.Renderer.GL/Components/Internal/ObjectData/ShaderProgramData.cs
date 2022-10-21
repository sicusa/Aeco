namespace Aeco.Renderer.GL;

using System.Collections.Immutable;

public struct UniformLocations
{
    public EnumArray<TextureType, int>? Textures;
    public ImmutableDictionary<string, int> Custom;

    public int FramebufferBlock;
    public int CameraBlock;
    public int MainLightBlock;
    public int MaterialBlock;
    public int ObjectBlock;
    public int MeshBlock;
}

public enum UniformBlockBinding
{
    Framebuffer = 1,
    Camera,
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
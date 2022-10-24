namespace Aeco.Renderer.GL;

using System.Collections.Immutable;

public struct BlockLocations
{
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
    public BlockLocations BlockLocations;
    public EnumArray<TextureType, int>? TextureLocations;
    public EnumArray<ShaderType, ImmutableDictionary<string, int>>? SubroutineLoactions;
    public ImmutableDictionary<string, int> CustomLocations;
    public int DepthBufferLocation;

    public void Dispose() { this = new(); }
}
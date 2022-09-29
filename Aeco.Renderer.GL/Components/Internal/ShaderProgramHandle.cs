namespace Aeco.Renderer.GL;

public struct UniformLocations
{
    public int World;
    public int View;
    public int Projection;
}

public struct ShaderProgramHandle : IGLObject
{
    public int Value;
    public UniformLocations UniformLocations;

    public void Dispose() { this = new(); }
}
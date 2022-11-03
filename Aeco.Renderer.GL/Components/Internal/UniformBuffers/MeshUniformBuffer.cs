namespace Aeco.Renderer.GL;

public struct MeshUniformBuffer : IGLObject
{
    public int Handle;

    public void Dispose() => this = new();
}
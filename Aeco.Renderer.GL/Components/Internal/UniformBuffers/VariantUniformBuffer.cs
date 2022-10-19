namespace Aeco.Renderer.GL;

public struct VariantUniformBuffer : IGLObject
{
    public int Handle;

    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

public struct VariantUniformBufferHandle : IGLObject
{
    public int Value;

    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

public struct ObjectUniformBufferHandle : IGLObject
{
    public int Value;

    public void Dispose() { this = new(); }
}
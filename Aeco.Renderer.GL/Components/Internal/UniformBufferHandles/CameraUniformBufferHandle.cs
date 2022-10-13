namespace Aeco.Renderer.GL;

public struct CameraUniformBufferHandle : IGLObject
{
    public int Value;

    public void Dispose() { this = new(); }
}
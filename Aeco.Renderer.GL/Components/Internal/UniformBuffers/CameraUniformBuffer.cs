namespace Aeco.Renderer.GL;

public struct CameraUniformBuffer : IGLObject
{
    public int Handle;

    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

public struct VariantUniformBuffer : IGLObject
{
    public int Handle;
    public IntPtr Pointer;

    public void Dispose() => this = new();
}
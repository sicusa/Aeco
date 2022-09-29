namespace Aeco.Renderer.GL;

public struct AppliedParent : IGLObject
{
    public Guid Id;

    public void Dispose() { this = new(); }
}
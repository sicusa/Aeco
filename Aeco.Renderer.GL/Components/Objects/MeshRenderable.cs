namespace Aeco.Renderer.GL;

public struct MeshRenderable : IGLReactiveObject
{
    public MeshResource Mesh;
    public bool IsVariant;

    public void Dispose() { this = new(); }
}
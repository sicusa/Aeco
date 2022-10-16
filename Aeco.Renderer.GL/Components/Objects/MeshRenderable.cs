namespace Aeco.Renderer.GL;

public struct MeshRenderable : IGLReactiveObject
{
    public MeshResource Mesh;

    public void Dispose() { this = new(); }
}
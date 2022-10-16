namespace Aeco.Renderer.GL;

public class MeshRenderableManager : ObjectManagerBase<MeshRenderable, MeshRenderableData>
{
    protected override void Initialize(IDataLayer<IComponent> context, Guid id, ref MeshRenderable renderable, ref MeshRenderableData data, bool updating)
    {
        if (!updating) {
            Uninitialize(context, id, in renderable, in data);
        }
        data.MeshId = ResourceLibrary<MeshResource>.Reference<Mesh>(context, renderable.Mesh, id);
        context.Acquire<MeshRendering>(data.MeshId);
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in MeshRenderable obj, in MeshRenderableData data)
    {
        ResourceLibrary<MeshResource>.Unreference(context, data.MeshId, id, out int newRefCount);
        if (newRefCount == 0) {
            context.Remove<MeshRendering>(data.MeshId);
        }
    }
}
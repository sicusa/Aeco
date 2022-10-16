namespace Aeco.Renderer.GL;

public class MeshRenderableManager : ObjectManagerBase<MeshRenderable, MeshRenderableData>
{
    protected override void Initialize(IDataLayer<IComponent> context, Guid id, ref MeshRenderable renderable, ref MeshRenderableData data, bool updating)
    {
        if (!updating) {
            Uninitialize(context, id, in renderable, in data);
        }
        data.MeshId = ResourceLibrary<MeshResource>.Reference<Mesh>(context, renderable.Mesh, id);

        ref var list = ref context.Acquire<RenderingList>(data.MeshId);
        list.Ids = list.Ids.Add(id);
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in MeshRenderable obj, in MeshRenderableData data)
    {
        ResourceLibrary<MeshResource>.Unreference(context, data.MeshId, id);

        ref var list = ref context.Acquire<RenderingList>(data.MeshId);
        list.Ids = list.Ids.Remove(id);

        if (list.Ids.Length == 0) {
            context.Remove<RenderingList>(data.MeshId);
        }
    }
}
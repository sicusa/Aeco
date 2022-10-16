namespace Aeco.Renderer.GL;

public class MeshRenderableManager : ObjectManagerBase<MeshRenderable, MeshRenderableData>
{
    protected override void Initialize(IDataLayer<IComponent> context, Guid id, ref MeshRenderable renderable, ref MeshRenderableData data, bool updating)
    {
        if (!updating) {
            Uninitialize(context, id, in renderable, in data);
        }

        data.MeshId = ResourceLibrary<MeshResource>.Reference<Mesh>(context, renderable.Mesh, id);
        context.Acquire<MeshInstancesDirty>(data.MeshId);

        ref var state = ref context.Acquire<MeshRenderingState>(data.MeshId);
        if (renderable.IsVariant) {
            state.VariantIds.Add(id);
            data.InstanceId = -1;
        }
        else {
            data.InstanceId = state.Instances.Count;
        }
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in MeshRenderable renderable, in MeshRenderableData data)
    {
        ResourceLibrary<MeshResource>.Unreference(context, data.MeshId, id);

        ref var state = ref context.Acquire<MeshRenderingState>(data.MeshId);
        if (data.InstanceId == -1) {
            state.VariantIds.Remove(id);
        }
        else {

        }
    }

    private void UpdateMeshInstances()
    {

    }
}
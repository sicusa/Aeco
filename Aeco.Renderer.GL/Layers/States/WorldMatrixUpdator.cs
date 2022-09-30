namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public class WorldMatrixUpdator : VirtualLayer, IGLUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        Traverse(context, GLRendererLayer.RootId);
        context.RemoveAll<ChildrenTransformMatricesDirty>();
    }

    private void Traverse(IDataLayer<IComponent> context, Guid id)
    {
        if (!context.TryGet<Children>(id, out var children)) {
            return;
        }
        ref var matrices = ref context.Acquire<TransformMatrices>(id);
        foreach (var childId in children.Ids) {
            if (context.Contains<TransformMatricesDirty>(childId)) {
                ref var childMatrices = ref context.Acquire<TransformMatrices>(id);
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                childMatrices.World = matrices.Combined * matrices.World;
                UpdateRecursively(context, childId, ref childMatrices);
            }
            else if (context.Contains<ChildrenTransformMatricesDirty>()) {
                Traverse(context, id);
            }
        }
    }

    private void UpdateRecursively(IDataLayer<IComponent> context, Guid id, ref TransformMatrices matrices)
    {
        context.Acquire<WorldViewChanged>(id);
        context.Remove<TransformMatricesDirty>(id);

        if (context.TryGet<Children>(id, out var children)) {
            foreach (var childId in children.Ids) {
                ref var childMatrices = ref context.Acquire<TransformMatrices>(childId);
                if (context.Contains<TransformMatricesDirty>(childId)) {
                    childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                }
                childMatrices.World = childMatrices.Combined * matrices.World;
                UpdateRecursively(context, childId, ref childMatrices);
            }
        }
    }
}
namespace Aeco.Renderer.GL;

public class WorldMatrixUpdator : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        if (!context.TryGet<Children>(GLRenderer.RootId, out var children)) {
            return;
        }
        foreach (var childId in children.Ids) {
            if (context.Contains<TransformMatricesDirty>(childId)) {
                ref var childMatrices = ref context.Acquire<TransformMatrices>(childId);
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                childMatrices.WorldRaw = childMatrices.Combined;
                UpdateRecursively(context, childId, ref childMatrices);
            }
            else if (context.Contains<ChildrenTransformMatricesDirty>(childId)) {
                Traverse(context, childId);
            }
        }
    }

    public void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        context.RemoveAll<TransformMatricesDirty>();
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
                ref var childMatrices = ref context.Acquire<TransformMatrices>(childId);
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                childMatrices.WorldRaw = childMatrices.Combined * matrices.WorldRaw;
                UpdateRecursively(context, childId, ref childMatrices);
            }
            else if (context.Contains<ChildrenTransformMatricesDirty>(childId)) {
                Traverse(context, childId);
            }
        }
    }

    private void UpdateRecursively(IDataLayer<IComponent> context, Guid id, ref TransformMatrices matrices)
    {
        context.Remove<WorldView>(id);
        context.Remove<WorldPosition>(id);
        context.Remove<WorldRotation>(id);

        if (context.TryGet<Children>(id, out var children)) {
            foreach (var childId in children.Ids) {
                ref var childMatrices = ref context.Acquire<TransformMatrices>(childId);
                context.Acquire<TransformMatricesDirty>(childId, out bool exists);
                if (exists) {
                    childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                }
                childMatrices.WorldRaw = childMatrices.Combined * matrices.WorldRaw;
                UpdateRecursively(context, childId, ref childMatrices);
            }
        }
    }
}
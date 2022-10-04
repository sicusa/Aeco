namespace Aeco.Renderer.GL;

using System.Numerics;

public class WorldMatrixUpdator : VirtualLayer, IGLUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        Traverse(context, GLRenderer.RootId);
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
                childMatrices.WorldRaw = childMatrices.Combined * matrices.World;
                Matrix4x4.Invert(childMatrices.WorldRaw, out childMatrices.ObjectRaw);
                UpdateRecursively(context, childId, ref childMatrices);
            }
            else if (context.Contains<ChildrenTransformMatricesDirty>(childId)) {
                Traverse(context, childId);
            }
        }
    }

    private void UpdateRecursively(IDataLayer<IComponent> context, Guid id, ref TransformMatrices matrices)
    {
        context.Remove<TransformMatricesDirty>(id);

        context.Acquire<WorldViewDirty>(id);
        context.Acquire<WorldPositionDirty>(id);
        context.Acquire<WorldRotationDirty>(id);

        if (context.TryGet<Children>(id, out var children)) {
            foreach (var childId in children.Ids) {
                ref var childMatrices = ref context.Acquire<TransformMatrices>(childId);
                if (context.Contains<TransformMatricesDirty>(childId)) {
                    childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                }
                childMatrices.WorldRaw = childMatrices.Combined * matrices.World;
                Matrix4x4.Invert(childMatrices.WorldRaw, out childMatrices.ObjectRaw);
                UpdateRecursively(context, childId, ref childMatrices);
            }
        }
    }
}
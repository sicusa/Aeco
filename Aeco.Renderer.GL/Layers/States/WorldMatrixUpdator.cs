namespace Aeco.Renderer.GL;

using System.Collections.Immutable;

public class WorldMatrixUpdator : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
        => Traverse(context, GLRenderer.RootId);

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
        var childrenIds = children.Ids;
        if (childrenIds.Count > 64) {
            var matricesCopy = matrices;
            var bundleSize = childrenIds.Count / 4;
            Parallel.Invoke(
                () => DoTraverse(childrenIds, 0, bundleSize, context, matricesCopy),
                () => DoTraverse(childrenIds, bundleSize, 2 * bundleSize, context, matricesCopy),
                () => DoTraverse(childrenIds, 2 * bundleSize, 3 * bundleSize, context, matricesCopy),
                () => DoTraverse(childrenIds, 3 * bundleSize, childrenIds.Count, context, matricesCopy));
        }
        else {
            DoTraverse(childrenIds, 0, childrenIds.Count, context, in matrices);
        }
    }

    private void DoTraverse(ImmutableList<Guid> childrenIds, int start, int end, IDataLayer<IComponent> context, in TransformMatrices matrices)
    {
        for (int i = start; i != end; ++i) {
            var childId = childrenIds[i];
            if (context.Contains<TransformMatricesDirty>(childId)) {
                ref var childMatrices = ref context.Acquire<TransformMatrices>(childId);
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                childMatrices.World = childMatrices.Combined * matrices.World;
                UpdateRecursively(context, childId, ref childMatrices);
            }
            else if (context.Contains<ChildrenTransformMatricesDirty>(childId)) {
                Traverse(context, childId);
            }
        }
    }

    private void UpdateRecursively(IDataLayer<IComponent> context, Guid id, ref TransformMatrices matrices)
    {
        context.Remove<WorldAxes>(id);
        context.Remove<WorldPosition>(id);
        context.Remove<WorldRotation>(id);

        if (context.TryGet<Children>(id, out var children)) {
            var childrenIds = children.Ids;
            if (childrenIds.Count > 64) {
                var matricesCopy = matrices;
                var bundleSize = childrenIds.Count / 4;
                Parallel.Invoke(
                    () => DoUpdate(childrenIds, 0, bundleSize, context, matricesCopy),
                    () => DoUpdate(childrenIds, bundleSize, 2 * bundleSize, context, matricesCopy),
                    () => DoUpdate(childrenIds, 2 * bundleSize, 3 * bundleSize, context, matricesCopy),
                    () => DoUpdate(childrenIds, 3 * bundleSize, childrenIds.Count, context, matricesCopy));
            }
            else {
                DoUpdate(childrenIds, 0, childrenIds.Count, context, matrices);
            }
        }
    }

    private void DoUpdate(ImmutableList<Guid> childrenIds, int start, int end, IDataLayer<IComponent> context, in TransformMatrices matrices)
    {
        for (int i = start; i != end; ++i) {
            var childId = childrenIds[i];
            ref var childMatrices = ref context.Require<TransformMatrices>(childId);
            context.Acquire<TransformMatricesDirty>(childId, out bool exists);
            if (!exists) {
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
            }
            childMatrices.World = childMatrices.Combined * matrices.World;
            UpdateRecursively(context, childId, ref childMatrices);
        }
    }
}
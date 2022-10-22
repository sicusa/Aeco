namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Collections.Immutable;
using System.Collections.Concurrent;

public class TransformMatricesUpdator : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
{
    private const int ParallelCount = 8;
    private ConcurrentBag<Guid> _modifiedIds = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        Traverse(context, GLRenderer.RootId);

        if (_modifiedIds.Count != 0) {
            foreach (var id in _modifiedIds) {
                context.Remove<WorldAxes>(id);
                context.Remove<WorldPosition>(id);
                context.Remove<WorldRotation>(id);
                context.Acquire<TransformMatricesDirty>(id);
            }
            _modifiedIds.Clear();
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
        var worldMatrix = matrices.World;
        var childrenIds = children.Ids;

        if (childrenIds.Count > 64) {
            var bundleSize = childrenIds.Count / ParallelCount;
            var actions = new Action[ParallelCount];

            for (int i = 0; i != ParallelCount; ++i) {
                int start = i * bundleSize;
                int end = i == ParallelCount - 1 ? childrenIds.Count : start + bundleSize;
                actions[i] = () => DoTraverse(childrenIds, start, end, context, in worldMatrix);
            }
            Parallel.Invoke(actions);
        }
        else {
            DoTraverse(childrenIds, 0, childrenIds.Count, context, in worldMatrix);
        }
    }

    private void DoTraverse(ImmutableList<Guid> childrenIds, int start, int end, IDataLayer<IComponent> context, in Matrix4x4 worldMatrix)
    {
        for (int i = start; i != end; ++i) {
            var childId = childrenIds[i];
            if (context.Contains<TransformMatricesDirty>(childId)) {
                ref TransformMatrices childMatrices = ref context.Require<TransformMatrices>(childId);
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
                childMatrices.World = childMatrices.Combined * worldMatrix;
                UpdateRecursively(context, childId, ref childMatrices);
            }
            else if (context.Contains<ChildrenTransformMatricesDirty>(childId)) {
                Traverse(context, childId);
            }
        }
    }

    private void UpdateRecursively(IDataLayer<IComponent> context, Guid id, ref TransformMatrices matrices)
    {
        _modifiedIds.Add(id);

        if (context.TryGet<Children>(id, out var children)) {
            var childrenIds = children.Ids;
            var worldMatrix = matrices.World;

            if (childrenIds.Count > 64) {
                var bundleSize = childrenIds.Count / ParallelCount;
                var actions = new Action[ParallelCount];

                for (int i = 0; i != ParallelCount; ++i) {
                    int start = i * bundleSize;
                    int end = i == ParallelCount - 1 ? childrenIds.Count : start + bundleSize;
                    actions[i] = () => DoUpdate(childrenIds, start, end, context, in worldMatrix);
                }
                Parallel.Invoke(actions);
            }
            else {
                DoUpdate(childrenIds, 0, childrenIds.Count, context, in worldMatrix);
            }
        }
    }

    private void DoUpdate(ImmutableList<Guid> childrenIds, int start, int end, IDataLayer<IComponent> context, in Matrix4x4 worldMatrix)
    {
        for (int i = start; i != end; ++i) {
            var childId = childrenIds[i];
            ref var childMatrices = ref context.Require<TransformMatrices>(childId);
            if (context.Contains<TransformMatricesDirty>(childId)) {
                childMatrices.Combined = childMatrices.Scale * childMatrices.Rotation * childMatrices.Translation;
            }
            childMatrices.World = childMatrices.Combined * worldMatrix;
            UpdateRecursively(context, childId, ref childMatrices);
        }
    }
}
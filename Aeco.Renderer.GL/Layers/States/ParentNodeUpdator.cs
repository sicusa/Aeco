namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public class ParentNodeUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<Modified<Parent>, Parent> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<Parent>>()) {
            if (context.Contains<Parent>(id) ||
                    !context.Remove<AppliedParent>(id, out var parent)) {
                continue;
            }

            ref var matrices = ref context.Acquire<TransformMatrices>(id);
            matrices.WorldRaw = matrices.Combined;

            if (parent.Id == Guid.Empty) {
                Console.WriteLine("Internal error: applied parent ID is empty.");
                continue;
            }
            ref var children = ref context.Acquire<Children>(parent.Id);
            children.Ids = children.Ids.Remove(id);
            TransformMatricesDirty.Tag(context, id);
        }

        foreach (var id in _q.Query(context)) {
            ref var appliedParent = ref context.Acquire<AppliedParent>(id, out bool exists);
            if (exists) {
                ref var prevParentChildren = ref context.Require<Children>(appliedParent.Id);
                prevParentChildren.Ids = prevParentChildren.Ids.Remove(id);
            }

            ref readonly var parent = ref context.Inspect<Parent>(id);
            if (parent.Id == Guid.Empty) {
                Console.WriteLine("Parent ID should not be empty.");
                continue;
            }
            appliedParent.Id = parent.Id;

            ref var children = ref context.Acquire<Children>(parent.Id);
            children.Ids = children.Ids.Add(id);
            TransformMatricesDirty.Tag(context, id);
        }
    }
}
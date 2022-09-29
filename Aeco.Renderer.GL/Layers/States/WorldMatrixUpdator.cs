namespace Aeco.Renderer.GL;

public class WorldMatrixUpdator : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
{
    private List<Guid> _ids = new();
    private HashSet<Guid> _updatedIds = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        _ids.AddRange(context.Query<TransformMatricesChanged>());

        foreach (var id in _ids) {
            if (_updatedIds.Contains(id)) { continue; }
            _updatedIds.Add(id);
            context.Acquire<WorldViewChanged>(id);

            ref var matrices = ref context.Acquire<TransformMatrices>(id);
            matrices.Combined = matrices.Scale * matrices.Rotation * matrices.Translation;

            if (context.TryGet<AppliedParent>(id, out var parent)) {
                ref readonly var parentMatrices = ref context.Inspect<TransformMatrices>(parent.Id);
                matrices.World = matrices.Combined * parentMatrices.World;
            }
            else {
                matrices.World = matrices.Combined;
            }

            if (context.TryGet<Children>(id, out var children)) {
                foreach (var child in children.Ids) {
                    Recurse(context, child, matrices);
                }
            }
        }

        _ids.Clear();
    }

    private void Recurse(IDataLayer<IComponent> context, Guid id, in TransformMatrices parentMatrices)
    {
        if (_updatedIds.Contains(id)) { return; }
        _updatedIds.Add(id);
        context.Acquire<WorldViewChanged>(id);
        context.Acquire<TransformMatricesChanged>(id, out bool exists);

        ref var matrices = ref context.Acquire<TransformMatrices>(id);
        if (exists) {
            matrices.Combined = matrices.Scale * matrices.Rotation * matrices.Translation;
        }
        matrices.World = matrices.Combined * parentMatrices.World;

        if (context.TryGet<Children>(id, out var children)) {
            foreach (var child in children.Ids) {
                Recurse(context, child, matrices);
            }
        }
    }

    public void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _updatedIds) {
            context.Remove<TransformMatricesChanged>(id);
        }
        _updatedIds.Clear();
    }
}
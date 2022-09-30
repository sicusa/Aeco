namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public abstract class MatrixUpdatorBase<TProperty> : VirtualLayer, IGLUpdateLayer
    where TProperty : IComponent, new()
{
    private Query<Modified<TProperty>, TProperty> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var property = ref context.Inspect<TProperty>(id);
            ref var matrices = ref context.Acquire<TransformMatrices>(id);
            UpdateMatrices(ref matrices, property);
            TransformMatricesDirty.Tag(context, id);
        }
    }

    protected abstract void UpdateMatrices(ref TransformMatrices matrices, in TProperty property);
}
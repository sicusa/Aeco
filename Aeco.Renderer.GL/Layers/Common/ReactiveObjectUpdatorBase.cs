namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public abstract class ReactiveObjectUpdatorBase<TObject> : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
    where TObject : IGLReactiveObject
{
    private Query<TObject, Destroy> _destroy_q = new();

    public virtual void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<TObject>()) {
            if (context.Contains<Modified<TObject>>(id)) {
                UpdateObject(context, id);
            }
        }
    }

    public virtual void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _destroy_q.Query(context)) {
            DestroyObject(context, id);
        }
    }

    protected abstract void UpdateObject(IDataLayer<IComponent> context, Guid id);
    protected abstract void DestroyObject(IDataLayer<IComponent> context, Guid id);
}

public abstract class ReactiveObjectUpdatorBase<TObject, TDirtyTag> : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
    where TObject : IGLReactiveObject
    where TDirtyTag : IGLObject
{
    private Query<TObject, Destroy> _destroy_q = new();

    public virtual void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<TObject>()) {
            if (context.Contains<TDirtyTag>(id) || context.Contains<Modified<TObject>>(id)) {
                UpdateObject(context, id);
            }
        }
    }

    public virtual void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _destroy_q.Query(context)) {
            ReleaseObject(context, id);
        }
    }

    protected abstract void UpdateObject(IDataLayer<IComponent> context, Guid id);
    protected abstract void ReleaseObject(IDataLayer<IComponent> context, Guid id);
}
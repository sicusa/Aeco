namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Diagnostics.CodeAnalysis;

using Aeco.Reactive;

public abstract class DelayedReactiveStorageBase<TComponent>
    : DelayedStorageBase<TComponent>, IGLUpdateLayer
    where TComponent : IComponent, IDisposable, new()
{
    private Query<Modified<TComponent>, TComponent> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref var comp = ref context.UnsafeInspect<TComponent>(id);
            OnModified(id, ref comp);
        }
    }

    protected abstract void OnModified(Guid id, ref TComponent comp);
}
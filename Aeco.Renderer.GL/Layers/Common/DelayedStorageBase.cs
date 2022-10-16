namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Diagnostics.CodeAnalysis;

using Aeco.Local;

public abstract class DelayedStorageBase<TComponent, TDirtyTag> : MonoPoolStorage<TComponent>, IGLLoadLayer
    where TComponent : IComponent, IDisposable, new()
    where TDirtyTag : IComponent
{
    [AllowNull]
    protected IDataLayer<IComponent> Context { get; private set; }

    public void OnLoad(IDataLayer<IComponent> context)
        => Context = context;

    public override ref TComponent Acquire(Guid entityId, out bool exists)
    {
        ref TComponent comp = ref base.Acquire(entityId, out exists);
        if (!exists || Context.Contains<TDirtyTag>(entityId)) {
            OnRefresh(entityId, ref comp);
            Context.Remove<TDirtyTag>(entityId);
        }
        return ref comp;
    }

    public override sealed ref TComponent Require(Guid entityId)
        => ref Acquire(entityId);

    public override sealed ref readonly TComponent Inspect(Guid entityId)
        => ref Acquire(entityId);

    protected abstract void OnRefresh(Guid id, ref TComponent comp);
}

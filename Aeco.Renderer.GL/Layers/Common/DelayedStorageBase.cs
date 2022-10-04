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
            OnRefrash(entityId, ref comp);
            Context.Remove<TDirtyTag>(entityId);
        }
        return ref comp;
    }

    public override ref TComponent Require(Guid entityId)
        => ref Acquire(entityId);

    public override ref readonly TComponent Inspect(Guid entityId)
        => ref Acquire(entityId);

    public override ref TComponent UnsafeAcquire(Guid entityId)
        => ref base.Acquire(entityId);

    public override ref TComponent UnsafeAcquire(Guid entityId, out bool exists)
        => ref base.Acquire(entityId, out exists);

    public override ref TComponent UnsafeInspect(Guid entityId)
        => ref base.Require(entityId);

    protected abstract void OnRefrash(Guid id, ref TComponent comp);
}

namespace Aeco.Local;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public abstract class LocalMonoDataLayerBase<TComponent, TStoredComponent>
    : LocalDataLayerBase<TComponent, TStoredComponent>, IMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    public override bool CheckSupported(Type componentType)
        => typeof(TStoredComponent) == componentType;

    public IReadOnlyEntity<TStoredComponent> GetReadOnlyEntity()
        => (IReadOnlyEntity<TStoredComponent>)GetReadOnlyEntity(Singleton());
    public IEntity<TStoredComponent> GetEntity()
        => (IEntity<TStoredComponent>)GetEntity(Singleton());

    public abstract bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component);
    public virtual ref readonly TStoredComponent Inspect(Guid entityId)
        => ref Require(entityId);
    public ref readonly TStoredComponent InspectAny()
        => ref Inspect(Singleton());
    public virtual ref TStoredComponent UnsafeInspect(Guid entityId)
        => ref Require(entityId);
    public ref TStoredComponent UnsafeInspectAny()
        => ref UnsafeInspect(Singleton());
    public abstract bool Contains(Guid entityId);
    public abstract bool ContainsAny();

    public abstract Guid Singleton();

    public abstract ref TStoredComponent Require(Guid entityId);
    public ref TStoredComponent RequireAny()
        => ref Require(Singleton());

    public abstract ref TStoredComponent Acquire(Guid entityId);
    public abstract ref TStoredComponent Acquire(Guid entityId, out bool exists);
    public virtual ref TStoredComponent UnsafeAcquire(Guid entityId)
        => ref Acquire(entityId);
    public virtual ref TStoredComponent UnsafeAcquire(Guid entityId, out bool exists)
        => ref Acquire(entityId, out exists);

    public abstract bool Remove(Guid entityId);
    public bool RemoveAny()
        => Remove(Singleton());
    public abstract bool Remove(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component);
    public bool RemoveAny([MaybeNullWhen(false)] out TStoredComponent component)
        => Remove(Singleton(), out component);

    public abstract ref TStoredComponent Set(Guid entityId, in TStoredComponent component);
    public ref TStoredComponent SetAny(in TStoredComponent component)
        => ref Set(Singleton(), component);
    
    private IMonoDataLayer<TComponent, UComponent> Convert<UComponent>()
        where UComponent : TComponent
        => this as IMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    public sealed override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        => Convert<UComponent>().TryGet(entityId, out component);
    public sealed override ref readonly UComponent Inspect<UComponent>(Guid entityId)
        => ref Convert<UComponent>().Inspect(entityId);
    public sealed override ref UComponent UnsafeInspect<UComponent>(Guid entityId)
        => ref Convert<UComponent>().UnsafeInspect(entityId);
    public sealed override bool Contains<UComponent>(Guid entityId)
        => Convert<UComponent>().Contains(entityId);
    public sealed override bool ContainsAny<UComponent>()
        => Convert<UComponent>().ContainsAny();
    public sealed override Guid Singleton<UComponent>()
        => Convert<UComponent>().Singleton();
    public sealed override ref UComponent Require<UComponent>(Guid entityId)
        => ref Convert<UComponent>().Require(entityId);
    public sealed override ref UComponent Acquire<UComponent>(Guid entityId)
        => ref Convert<UComponent>().Acquire(entityId);
    public sealed override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        => ref Convert<UComponent>().Acquire(entityId, out exists);
    public sealed override ref UComponent UnsafeAcquire<UComponent>(Guid entityId)
        => ref Convert<UComponent>().UnsafeAcquire(entityId);
    public sealed override ref UComponent UnsafeAcquire<UComponent>(Guid entityId, out bool exists)
        => ref Convert<UComponent>().UnsafeAcquire(entityId, out exists);
    public sealed override bool Remove<UComponent>(Guid entityId)
        => Convert<UComponent>().Remove(entityId);
    public sealed override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        => Convert<UComponent>().Remove(entityId, out component);
    public sealed override void RemoveAll<UComponent>()
        => Convert<UComponent>().Clear();
    public sealed override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
        => ref Convert<UComponent>().Set(entityId, component);
    public sealed override IEnumerable<Guid> Query<UComponent>()
        => Convert<UComponent>().Query();
}
namespace Aeco.Local;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public abstract class LocalMonoDataLayerBase<TComponent, TStoredComponent>
    : LocalDataLayerBase<TComponent, TStoredComponent>, IMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    public override bool CheckSupported(Type componentType)
        => typeof(TStoredComponent) == componentType;
    
    protected Guid RequireSingleton()
        => Singleton() ?? throw new KeyNotFoundException("Singleton not found: " + typeof(TStoredComponent));

    public abstract bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    public virtual ref readonly TStoredComponent Inspect(Guid id)
        => ref Require(id);
    public ref readonly TStoredComponent InspectAny()
        => ref Inspect(RequireSingleton());
    public virtual ref TStoredComponent InspectRaw(Guid id)
        => ref Require(id);
    public ref TStoredComponent InspectAnyRaw()
        => ref InspectRaw(RequireSingleton());
    public abstract bool Contains(Guid id);
    public abstract bool ContainsAny();

    public abstract Guid? Singleton();

    public abstract ref TStoredComponent Require(Guid id);
    public ref TStoredComponent RequireAny()
        => ref Require(RequireSingleton());

    public abstract ref TStoredComponent Acquire(Guid id);
    public abstract ref TStoredComponent Acquire(Guid id, out bool exists);
    public ref TStoredComponent AcquireAny()
        => ref Acquire(Singleton() ?? Guid.NewGuid());
    public ref TStoredComponent AcquireAny(out bool exists)
        => ref Acquire(Singleton() ?? Guid.NewGuid(), out exists);
    public virtual ref TStoredComponent AcquireRaw(Guid id)
        => ref Acquire(id);
    public virtual ref TStoredComponent AcquireRaw(Guid id, out bool exists)
        => ref Acquire(id, out exists);

    public abstract bool Remove(Guid id);
    public bool RemoveAny()
    {
        var singleton = Singleton();
        return singleton != null ? Remove(singleton.Value) : false;
    }
    public abstract bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    public bool RemoveAny([MaybeNullWhen(false)] out TStoredComponent component)
    {
        var singleton = Singleton();
        if (singleton == null) {
            component = default;
            return false;
        }
        return Remove(singleton.Value, out component);
    }

    public abstract ref TStoredComponent Set(Guid id, in TStoredComponent component);
    public ref TStoredComponent SetAny(in TStoredComponent component)
        => ref Set(RequireSingleton(), component);

    public sealed override bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        => ConvertReadable<UComponent>().TryGet(id, out component);
    public sealed override ref readonly UComponent Inspect<UComponent>(Guid id)
        => ref ConvertReadable<UComponent>().Inspect(id);
    public sealed override ref UComponent InspectRaw<UComponent>(Guid id)
        => ref ConvertWritable<UComponent>().InspectRaw(id);
    public sealed override bool Contains<UComponent>(Guid id)
        => ConvertReadable<UComponent>().Contains(id);
    public sealed override bool ContainsAny<UComponent>()
        => ConvertReadable<UComponent>().ContainsAny();
    public sealed override Guid? Singleton<UComponent>()
        => ConvertReadable<UComponent>().Singleton();
    public sealed override ref UComponent Require<UComponent>(Guid id)
        => ref ConvertWritable<UComponent>().Require(id);
    public sealed override ref UComponent Acquire<UComponent>(Guid id)
        => ref ConvertExpandable<UComponent>().Acquire(id);
    public sealed override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        => ref ConvertExpandable<UComponent>().Acquire(id, out exists);
    public sealed override ref UComponent AcquireRaw<UComponent>(Guid id)
        => ref ConvertExpandable<UComponent>().AcquireRaw(id);
    public sealed override ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        => ref ConvertExpandable<UComponent>().AcquireRaw(id, out exists);
    public sealed override bool Remove<UComponent>(Guid id)
        => ConvertShrinkable<UComponent>().Remove(id);
    public sealed override bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        => ConvertShrinkable<UComponent>().Remove(id, out component);
    public sealed override void RemoveAll<UComponent>()
        => ConvertShrinkable<UComponent>().Clear();
    public sealed override ref UComponent Set<UComponent>(Guid id, in UComponent component)
        => ref ConvertSettable<UComponent>().Set(id, component);
    public sealed override int GetCount<UComponent>()
        => ConvertReadable<UComponent>().GetCount();
    public sealed override IEnumerable<Guid> Query<UComponent>()
        => ConvertReadable<UComponent>().Query();

    private IReadableMonoDataLayer<TComponent, UComponent> ConvertReadable<UComponent>()
        where UComponent : TComponent
        => (IReadableMonoDataLayer<TComponent, UComponent>)this;

    private IWritableMonoDataLayer<TComponent, UComponent> ConvertWritable<UComponent>()
        where UComponent : TComponent
        => (IWritableMonoDataLayer<TComponent, UComponent>)this;

    private IExpandableMonoDataLayer<TComponent, UComponent> ConvertExpandable<UComponent>()
        where UComponent : TComponent, new()
        => (IExpandableMonoDataLayer<TComponent, UComponent>)this;

    private ISettableMonoDataLayer<TComponent, UComponent> ConvertSettable<UComponent>()
        where UComponent : TComponent, new()
        => (ISettableMonoDataLayer<TComponent, UComponent>)this;

    private IShrinkableMonoDataLayer<TComponent, UComponent> ConvertShrinkable<UComponent>()
        where UComponent : TComponent
        => (IShrinkableMonoDataLayer<TComponent, UComponent>)this;
}
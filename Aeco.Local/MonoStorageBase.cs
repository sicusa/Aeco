namespace Aeco.Local;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public abstract class MonoStorageBase<TComponent, TStoredComponent>
    : DataLayerBase<TComponent, TStoredComponent>
    , IReadableDataLayer<TComponent>
    , ISettableDataLayer<TComponent>
    , IReferableDataLayer<TComponent>
    , IShrinkableDataLayer<TComponent>
    , IReadableMonoDataLayer<TComponent, TStoredComponent>
    , ISettableMonoDataLayer<TComponent, TStoredComponent>
    , IReferableMonoDataLayer<TComponent, TStoredComponent>
    , IShrinkableMonoDataLayer<TComponent, TStoredComponent>
    , IComponentRefHost<TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    public override bool CheckComponentSupported(Type componentType)
        => typeof(TStoredComponent) == componentType;

    public ref readonly TStoredComponent Inspect(Guid id) => ref Require(id);

    public abstract bool Contains(Guid id);
    public abstract bool ContainsAny();
    public abstract Guid? Singleton();

    public abstract IEnumerable<object> GetAll(Guid id);

    public abstract bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    public abstract ref TStoredComponent Require(Guid id);

    public abstract ref TStoredComponent Acquire(Guid id);
    public abstract ref TStoredComponent Acquire(Guid id, out bool exists);

    public abstract ref TStoredComponent Set(Guid id, in TStoredComponent component);

    public virtual ComponentRef<TStoredComponent> GetRef(Guid id)
        => new ComponentRef<TStoredComponent>(this, id);

    public bool IsRefValid(Guid refId)
        => Contains(refId);

    public ref TStoredComponent RequireRef(Guid refId)
        => ref Require(refId);

    public abstract bool Remove(Guid id);
    public abstract bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);

    public abstract void Clear(Guid id);
    public abstract void Clear();

    public sealed override bool Contains<UComponent>(Guid id)
        => ConvertBasic<UComponent>().Contains(id);
    public sealed override bool ContainsAny<UComponent>()
        => ConvertBasic<UComponent>().ContainsAny();
    public sealed override Guid? Singleton<UComponent>()
        => ConvertBasic<UComponent>().Singleton();
    public sealed override int GetCount<UComponent>()
        => ConvertBasic<UComponent>().GetCount();
    public sealed override IEnumerable<Guid> Query<UComponent>()
        => ConvertBasic<UComponent>().Query();

    public bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
        => ConvertReadable<UComponent>().TryGet(id, out component);
    public ref readonly UComponent Inspect<UComponent>(Guid id)
        where UComponent : TComponent
        => ref ConvertReadable<UComponent>().Inspect(id);

    public ref UComponent InspectRaw<UComponent>(Guid id)
        where UComponent : TComponent
        => ref ConvertWritable<UComponent>().InspectRaw(id);
    public ref UComponent Require<UComponent>(Guid id)
        where UComponent : TComponent
        => ref ConvertWritable<UComponent>().Require(id);

    public ref UComponent Acquire<UComponent>(Guid id)
        where UComponent : TComponent, new()
        => ref ConvertExpandable<UComponent>().Acquire(id);
    public ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
        => ref ConvertExpandable<UComponent>().Acquire(id, out exists);
    public ref UComponent AcquireRaw<UComponent>(Guid id)
        where UComponent : TComponent, new()
        => ref ConvertExpandable<UComponent>().AcquireRaw(id);
    public ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
        => ref ConvertExpandable<UComponent>().AcquireRaw(id, out exists);

    public ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent, new()
        => ref ConvertSettable<UComponent>().Set(id, component);

    public ComponentRef<UComponent> GetRef<UComponent>(Guid id)
        where UComponent : TComponent
        => ConvertReferable<UComponent>().GetRef(id);

    public bool Remove<UComponent>(Guid id)
        where UComponent : TComponent
        => ConvertShrinkable<UComponent>().Remove(id);
    public bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
        => ConvertShrinkable<UComponent>().Remove(id, out component);
    public void RemoveAll<UComponent>()
        where UComponent : TComponent
        => ConvertShrinkable<UComponent>().Clear();

    private IBasicMonoDataLayer<TComponent, UComponent> ConvertBasic<UComponent>()
        where UComponent : TComponent
        => this as IBasicMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    private IReadableMonoDataLayer<TComponent, UComponent> ConvertReadable<UComponent>()
        where UComponent : TComponent
        => this as IReadableMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    private IWritableMonoDataLayer<TComponent, UComponent> ConvertWritable<UComponent>()
        where UComponent : TComponent
        => this as IWritableMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    private IExpandableMonoDataLayer<TComponent, UComponent> ConvertExpandable<UComponent>()
        where UComponent : TComponent, new()
        => this as IExpandableMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    private ISettableMonoDataLayer<TComponent, UComponent> ConvertSettable<UComponent>()
        where UComponent : TComponent, new()
        => this as ISettableMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    private IReferableMonoDataLayer<TComponent, UComponent> ConvertReferable<UComponent>()
        where UComponent : TComponent
        => this as IReferableMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");

    private IShrinkableMonoDataLayer<TComponent, UComponent> ConvertShrinkable<UComponent>()
        where UComponent : TComponent
        => this as IShrinkableMonoDataLayer<TComponent, UComponent>
            ?? throw new NotSupportedException("Component not supported");
}
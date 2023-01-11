namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface IReadableMonoDataLayer<in TComponent, TStoredComponent>
    : IDataLayerBase<TComponent>
    where TStoredComponent : TComponent
{
    bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    ref readonly TStoredComponent Inspect(Guid id);
    ref readonly TStoredComponent InspectAny();
    bool Contains(Guid id);
    bool ContainsAny();

    int GetCount();
    IEnumerable<Guid> Query();

    Guid? Singleton();

    IEnumerable<object> GetAll(Guid id);
}

public interface IWritableMonoDataLayer<in TComponent, TStoredComponent>
    : IDataLayerBase<TComponent>
    where TStoredComponent : TComponent
{
    ref TStoredComponent InspectRaw(Guid id);
    ref TStoredComponent InspectAnyRaw();

    ref TStoredComponent Require(Guid id);
    ref TStoredComponent RequireAny();
}

public interface IExpandableMonoDataLayer<in TComponent, TStoredComponent>
    : IDataLayerBase<TComponent>
    where TStoredComponent : TComponent, new()
{
    ref TStoredComponent Acquire(Guid id);
    ref TStoredComponent Acquire(Guid id, out bool exists);
    ref TStoredComponent AcquireAny();
    ref TStoredComponent AcquireAny(out bool exists);
    ref TStoredComponent AcquireRaw(Guid id);
    ref TStoredComponent AcquireRaw(Guid id, out bool exists);
}

public interface ISettableMonoDataLayer<in TComponent, TStoredComponent>
    : IWritableMonoDataLayer<TComponent, TStoredComponent>
    , IExpandableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    ref TStoredComponent Set(Guid id, in TStoredComponent component);
    ref TStoredComponent SetAny(in TStoredComponent component);
}

public interface IShrinkableMonoDataLayer<in TComponent, TStoredComponent>
    : IShrinkableDataLayer<TComponent>
    where TStoredComponent : TComponent
{
    bool Remove(Guid id);
    bool RemoveAny();
    bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    bool RemoveAny([MaybeNullWhen(false)] out TStoredComponent component);
}

public interface IMonoDataLayer<in TComponent, TStoredComponent>
    : IReadableMonoDataLayer<TComponent, TStoredComponent>
    , ISettableMonoDataLayer<TComponent, TStoredComponent>
    , IShrinkableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
}
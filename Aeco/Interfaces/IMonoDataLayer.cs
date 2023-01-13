namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface IBasicMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicDataLayer<TComponent>
{
    bool Contains(Guid id);
    bool ContainsAny() => Singleton() != null;

    Guid? Singleton();
    Guid RequireSingleton() => Singleton()
        ?? throw new KeyNotFoundException("Singleton not found: " + typeof(TStoredComponent));
}

public interface IReadableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);

    ref readonly TStoredComponent Inspect(Guid id);
    ref readonly TStoredComponent InspectAny()
        => ref Inspect(RequireSingleton());

    IEnumerable<object> GetAll(Guid id);
}

public interface IWritableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    ref TStoredComponent Require(Guid id);
    ref TStoredComponent RequireAny()
        => ref Require(RequireSingleton());

    ref TStoredComponent InspectRaw(Guid id)
        => ref Require(id);
    ref TStoredComponent InspectAnyRaw()
        => ref InspectRaw(RequireSingleton());
}

public interface IExpandableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    ref TStoredComponent Acquire(Guid id);
    ref TStoredComponent AcquireAny()
        => ref Acquire(Singleton() ?? Guid.NewGuid());

    ref TStoredComponent Acquire(Guid id, out bool exists);
    ref TStoredComponent AcquireAny(out bool exists)
        => ref Acquire(Singleton() ?? Guid.NewGuid(), out exists);

    ref TStoredComponent AcquireRaw(Guid id)
        => ref Acquire(id);
    ref TStoredComponent AcquireAnyRaw(Guid id)
        => ref AcquireRaw(Singleton() ?? Guid.NewGuid());

    ref TStoredComponent AcquireRaw(Guid id, out bool exists)
        => ref Acquire(id, out exists);
    ref TStoredComponent AcquireAnyRaw(Guid id, out bool exists)
        => ref AcquireRaw(Singleton() ?? Guid.NewGuid(), out exists);
}

public interface ISettableMonoDataLayer<in TComponent, TStoredComponent>
    : IWritableMonoDataLayer<TComponent, TStoredComponent>
    , IExpandableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    ref TStoredComponent Set(Guid id, in TStoredComponent component);
    ref TStoredComponent SetAny(in TStoredComponent component)
        => ref Set(Singleton() ?? Guid.NewGuid(), component);
}

public interface IReferableMonoDataLayer<in TComponent, TStoredComponent>
    : IReadableMonoDataLayer<TComponent, TStoredComponent>
    , IWritableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    IComponentRef<TStoredComponent> GetRef(Guid id);
}

public interface IShrinkableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    bool Remove(Guid id);
    bool RemoveAny()
    {
        var id = Singleton();
        return id.HasValue ? Remove(id.Value) : false;
    }

    bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    bool RemoveAny([MaybeNullWhen(false)] out TStoredComponent component)
    {
        var id = Singleton();
        if (id.HasValue) {
            return Remove(id.Value, out component);
        }
        else {
            component = default;
            return false;
        }
    }

    void Clear();
}

public interface IMonoDataLayer<in TComponent, TStoredComponent>
    : IReadableMonoDataLayer<TComponent, TStoredComponent>
    , ISettableMonoDataLayer<TComponent, TStoredComponent>
    , IReferableMonoDataLayer<TComponent, TStoredComponent>
    , IShrinkableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
}
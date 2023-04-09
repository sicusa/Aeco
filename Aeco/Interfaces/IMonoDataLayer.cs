namespace Aeco;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public interface IBasicMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicDataLayer<TComponent>
{
    bool Contains(uint id);
    bool ContainsAny() => Singleton() != null;

    uint? Singleton();
    uint RequireSingleton() => Singleton()
        ?? throw new KeyNotFoundException("Singleton not found: " + typeof(TStoredComponent));
}

public interface IReadableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    bool TryGet(uint id, [MaybeNullWhen(false)] out TStoredComponent component);

    ref readonly TStoredComponent Inspect(uint id)
    {
        ref readonly TStoredComponent comp = ref InspectOrNullRef(id);
        if (Unsafe.IsNullRef(ref Unsafe.AsRef(in comp))) {
            throw ExceptionHelper.ComponentNotFound<TStoredComponent>();
        }
        return ref comp;
    }
    ref readonly TStoredComponent InspectAny()
        => ref Inspect(RequireSingleton());

    ref readonly TStoredComponent InspectOrNullRef(uint id);
    ref readonly TStoredComponent InspectAnyOrNullRef()
    {
        var singleton = Singleton();
        if (singleton == null) {
            return ref Unsafe.NullRef<TStoredComponent>();
        }
        return ref InspectOrNullRef(singleton.Value);
    }

    IEnumerable<object> GetAll(uint id);
}

public interface IWritableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    ref TStoredComponent Require(uint id)
    {
        ref TStoredComponent comp = ref RequireOrNullRef(id);
        if (Unsafe.IsNullRef(ref comp)) {
            throw ExceptionHelper.ComponentNotFound<TStoredComponent>();
        }
        return ref comp;
    }
    ref TStoredComponent RequireAny()
        => ref Require(RequireSingleton());

    ref TStoredComponent RequireOrNullRef(uint id);
    ref TStoredComponent RequireAnyOrNullRef()
    {
        var singleton = Singleton();
        if (singleton == null) {
            return ref Unsafe.NullRef<TStoredComponent>();
        }
        return ref RequireOrNullRef(singleton.Value);
    }

    ref TStoredComponent InspectRaw(uint id)
        => ref Require(id);
    ref TStoredComponent InspectAnyRaw()
        => ref InspectRaw(RequireSingleton());
}

public interface IExpandableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    ref TStoredComponent Acquire(uint id);
    ref TStoredComponent AcquireAny()
        => ref Acquire(Singleton() ?? IdFactory.New());

    ref TStoredComponent Acquire(uint id, out bool exists);
    ref TStoredComponent AcquireAny(out bool exists)
        => ref Acquire(Singleton() ?? IdFactory.New(), out exists);

    ref TStoredComponent AcquireRaw(uint id)
        => ref Acquire(id);
    ref TStoredComponent AcquireAnyRaw(uint id)
        => ref AcquireRaw(Singleton() ?? IdFactory.New());

    ref TStoredComponent AcquireRaw(uint id, out bool exists)
        => ref Acquire(id, out exists);
    ref TStoredComponent AcquireAnyRaw(uint id, out bool exists)
        => ref AcquireRaw(Singleton() ?? IdFactory.New(), out exists);
}

public interface ISettableMonoDataLayer<in TComponent, TStoredComponent>
    : IWritableMonoDataLayer<TComponent, TStoredComponent>
    , IExpandableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    ref TStoredComponent Set(uint id, in TStoredComponent component);
    ref TStoredComponent SetAny(in TStoredComponent component)
        => ref Set(Singleton() ?? IdFactory.New(), component);
}

public interface IReferableMonoDataLayer<in TComponent, TStoredComponent>
    : IReadableMonoDataLayer<TComponent, TStoredComponent>
    , IWritableMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    ComponentRef<TStoredComponent> GetRef(uint id);
}

public interface IShrinkableMonoDataLayer<in TComponent, TStoredComponent>
    : IBasicMonoDataLayer<TComponent, TStoredComponent>
    where TStoredComponent : TComponent
{
    bool Remove(uint id);
    bool RemoveAny()
    {
        var id = Singleton();
        return id.HasValue ? Remove(id.Value) : false;
    }

    bool Remove(uint id, [MaybeNullWhen(false)] out TStoredComponent component);
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
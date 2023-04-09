namespace Aeco;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public interface IBasicDataLayer<in TComponent> : ILayer<TComponent>
{
    bool CheckComponentSupported(Type componentType);

    bool Contains<UComponent>(uint id)
        where UComponent : TComponent;
    bool ContainsAny<UComponent>()
        where UComponent : TComponent
        => Singleton<UComponent>() != null;

    int GetCount();
    int GetCount<UComponent>()
        where UComponent : TComponent;

    IEnumerable<uint> Query();
    IEnumerable<uint> Query<UComponent>()
        where UComponent : TComponent;

    uint? Singleton<UComponent>()
        where UComponent : TComponent;
    uint RequireSingleton<UComponent>()
        where UComponent : TComponent
        => Singleton<UComponent>() ??
            throw ExceptionHelper.ComponentNotFound<UComponent>();
}

public interface IReadableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    bool TryGet<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;

    ref readonly UComponent Inspect<UComponent>(uint id)
        where UComponent : TComponent
    {
        ref readonly UComponent comp = ref InspectOrNullRef<UComponent>(id);
        if (Unsafe.IsNullRef(ref Unsafe.AsRef(in comp))) {
            throw ExceptionHelper.ComponentNotFound<UComponent>();
        }
        return ref comp;
    }
    ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent
        => ref Inspect<UComponent>(RequireSingleton<UComponent>());

    ref readonly UComponent InspectOrNullRef<UComponent>(uint id)
        where UComponent : TComponent;
    ref readonly UComponent InspectAnyOrNullRef<UComponent>()
        where UComponent : TComponent
    {
        var singleton = Singleton<UComponent>();
        if (singleton == null) {
            return ref Unsafe.NullRef<UComponent>();
        }
        return ref InspectOrNullRef<UComponent>(singleton.Value);
    }

    IEnumerable<object> GetAll(uint id);
}

public interface IWritableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    ref UComponent Require<UComponent>(uint id)
        where UComponent : TComponent
    {
        ref UComponent comp = ref RequireOrNullRef<UComponent>(id);
        if (Unsafe.IsNullRef(ref comp)) {
            throw ExceptionHelper.ComponentNotFound<UComponent>();
        }
        return ref comp;
    }
    ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent
        => ref Require<UComponent>(RequireSingleton<UComponent>());

    ref UComponent RequireOrNullRef<UComponent>(uint id)
        where UComponent : TComponent;
    ref UComponent RequireAnyOrNullRef<UComponent>()
        where UComponent : TComponent
    {
        var singleton = Singleton<UComponent>();
        if (singleton == null) {
            return ref Unsafe.NullRef<UComponent>();
        }
        return ref RequireOrNullRef<UComponent>(singleton.Value);
    }

    ref UComponent InspectRaw<UComponent>(uint id)
        where UComponent : TComponent
        => ref Require<UComponent>(id);
    ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent
        => ref InspectRaw<UComponent>(RequireSingleton<UComponent>());
}

public interface IExpandableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    ref UComponent Acquire<UComponent>(uint id)
        where UComponent : TComponent, new();
    ref UComponent AcquireAny<UComponent>()
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(Singleton<UComponent>() ?? IdFactory.New());

    ref UComponent Acquire<UComponent>(uint id, out bool exists)
        where UComponent : TComponent, new();
    ref UComponent AcquireAny<UComponent>(out bool exists)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(Singleton<UComponent>() ?? IdFactory.New(), out exists);

    ref UComponent AcquireRaw<UComponent>(uint id)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id);
    ref UComponent AcquireAnyRaw<UComponent>()
        where UComponent : TComponent, new()
        => ref AcquireRaw<UComponent>(Singleton<UComponent>() ?? IdFactory.New());

    ref UComponent AcquireRaw<UComponent>(uint id, out bool exists)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id, out exists);
    ref UComponent AcquireAnyRaw<UComponent>(out bool exists)
        where UComponent : TComponent, new()
        => ref AcquireRaw<UComponent>(Singleton<UComponent>() ?? IdFactory.New(), out exists);
}

public interface ISettableDataLayer<in TComponent>
    : IWritableDataLayer<TComponent>, IExpandableDataLayer<TComponent>
{
    ref UComponent Set<UComponent>(uint id, in UComponent component)
        where UComponent : TComponent, new();
    ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent, new()
        => ref Set<UComponent>(Singleton<UComponent>() ?? IdFactory.New(), component);
}

public interface IReferableDataLayer<in TComponent>
    : IReadableDataLayer<TComponent>, IWritableDataLayer<TComponent>
{
    ComponentRef<UComponent> GetRef<UComponent>(uint id)
        where UComponent : TComponent;
}

public interface IShrinkableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    bool Remove<UComponent>(uint id)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>()
        where UComponent : TComponent
    {
        var id = Singleton<UComponent>();
        return id.HasValue ? Remove<UComponent>(id.Value) : false;
    }

    bool Remove<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var id = Singleton<UComponent>();
        if (id.HasValue) {
            return Remove<UComponent>(id.Value, out component);
        }
        else {
            component = default;
            return false;
        }
    }

    void RemoveAll<UComponent>()
        where UComponent : TComponent;

    void Clear(uint id);
    void Clear();
}

public interface IDataLayer<in TComponent>
    : IReadableDataLayer<TComponent>
    , ISettableDataLayer<TComponent>
    , IReferableDataLayer<TComponent>
    , IShrinkableDataLayer<TComponent>
{
}
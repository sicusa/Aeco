namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface IBasicDataLayer<in TComponent> : ILayer<TComponent>
{
    bool CheckComponentSupported(Type componentType);

    bool Contains<UComponent>(Guid id)
        where UComponent : TComponent;
    bool ContainsAny<UComponent>()
        where UComponent : TComponent
        => Singleton<UComponent>() != null;

    int GetCount();
    int GetCount<UComponent>()
        where UComponent : TComponent;

    IEnumerable<Guid> Query();
    IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;

    Guid? Singleton<UComponent>()
        where UComponent : TComponent;
    Guid RequireSingleton<UComponent>()
        where UComponent : TComponent
        => Singleton<UComponent>()
            ?? throw new KeyNotFoundException("Singleton not found: " + typeof(UComponent));
}

public interface IReadableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;

    ref readonly UComponent Inspect<UComponent>(Guid id)
        where UComponent : TComponent;
    ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent
        => ref Inspect<UComponent>(RequireSingleton<UComponent>());

    IEnumerable<object> GetAll(Guid id);
}

public interface IWritableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    ref UComponent Require<UComponent>(Guid id)
        where UComponent : TComponent;
    ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent
        => ref Require<UComponent>(RequireSingleton<UComponent>());

    ref UComponent InspectRaw<UComponent>(Guid id)
        where UComponent : TComponent
        => ref Require<UComponent>(id);
    ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent
        => ref InspectRaw<UComponent>(RequireSingleton<UComponent>());
}

public interface IExpandableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    ref UComponent Acquire<UComponent>(Guid id)
        where UComponent : TComponent, new();
    ref UComponent AcquireAny<UComponent>()
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid());

    ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new();
    ref UComponent AcquireAny<UComponent>(out bool exists)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid(), out exists);

    ref UComponent AcquireRaw<UComponent>(Guid id)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id);
    ref UComponent AcquireAnyRaw<UComponent>(Guid id)
        where UComponent : TComponent, new()
        => ref AcquireRaw<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid());

    ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id, out exists);
    ref UComponent AcquireAnyRaw<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
        => ref AcquireRaw<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid(), out exists);
}

public interface ISettableDataLayer<in TComponent>
    : IWritableDataLayer<TComponent>, IExpandableDataLayer<TComponent>
{
    ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent, new();
    ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent, new()
        => ref Set<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid(), component);
}

public interface IReferableDataLayer<in TComponent>
    : IReadableDataLayer<TComponent>, IWritableDataLayer<TComponent>
{
    IComponentRef<UComponent> GetRef<UComponent>(Guid id)
        where UComponent : TComponent;
}

public interface IShrinkableDataLayer<in TComponent> : IBasicDataLayer<TComponent>
{
    bool Remove<UComponent>(Guid id)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>()
        where UComponent : TComponent
    {
        var id = Singleton<UComponent>();
        return id.HasValue ? Remove<UComponent>(id.Value) : false;
    }

    bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
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

    void Clear(Guid id);
    void Clear();
}

public interface IDataLayer<in TComponent>
    : IReadableDataLayer<TComponent>
    , ISettableDataLayer<TComponent>
    , IReferableDataLayer<TComponent>
    , IShrinkableDataLayer<TComponent>
{
}
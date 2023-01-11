namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface IDataLayerBase<in TComponent> : ILayer<TComponent>
{
    bool CheckSupported(Type componentType);
}

public interface IStableDataLayer<in TComponent> : IDataLayerBase<TComponent>
{
}

public interface IReadableDataLayer<in TComponent> : IDataLayerBase<TComponent>
{
    bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    ref readonly UComponent Inspect<UComponent>(Guid id)
        where UComponent : TComponent;
    ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent;
    bool Contains<UComponent>(Guid id)
        where UComponent : TComponent;
    bool ContainsAny<UComponent>()
        where UComponent : TComponent;

    int GetCount();
    int GetCount<UComponent>()
        where UComponent : TComponent;

    IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;
    IEnumerable<Guid> Query();

    Guid? Singleton<UComponent>()
        where UComponent : TComponent;

    IEnumerable<object> GetAll(Guid id);
}

public interface IWritableDataLayer<in TComponent> : IDataLayerBase<TComponent>
{
    ref UComponent InspectRaw<UComponent>(Guid id)
        where UComponent : TComponent;
    ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent;

    ref UComponent Require<UComponent>(Guid id)
        where UComponent : TComponent;
    ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent;
}

public interface IExpandableDataLayer<in TComponent> : IDataLayerBase<TComponent>
{
    ref UComponent Acquire<UComponent>(Guid id)
        where UComponent : TComponent, new();
    ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new();
    ref UComponent AcquireAny<UComponent>()
        where UComponent : TComponent, new();
    ref UComponent AcquireAny<UComponent>(out bool exists)
        where UComponent : TComponent, new();

    ref UComponent AcquireRaw<UComponent>(Guid id)
        where UComponent : TComponent, new();
    ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new();
}

public interface ISettableDataLayer<in TComponent>
    : IWritableDataLayer<TComponent>, IExpandableDataLayer<TComponent>
{
    ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent, new();
    ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent, new();
}

public interface IShrinkableDataLayer<in TComponent> : IDataLayerBase<TComponent>
{
    bool Remove<UComponent>(Guid id)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>()
        where UComponent : TComponent;
    bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    void RemoveAll<UComponent>()
        where UComponent : TComponent;

    void Clear(Guid id);
    void Clear();
}

public interface IDataLayer<in TComponent>
    : IReadableDataLayer<TComponent>
    , ISettableDataLayer<TComponent>
    , IShrinkableDataLayer<TComponent>
{
}
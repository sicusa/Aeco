namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface IReadOnlyLayer<in TComponent>
{
    IReadOnlyEntity<TComponent> GetReadOnlyEntity(Guid id);
}

public interface ILayer<in TComponent> : IReadOnlyLayer<TComponent>
{
    IEntity<TComponent> GetEntity(Guid id);
}

public interface IReadOnlyTrackableLayer<in TComponent> : IReadOnlyLayer<TComponent>
{
    IEnumerable<Guid> Entities { get; }
    bool ContainsEntity(Guid id);
}

public interface ITrackableLayer<in TComponent>
    : IReadOnlyTrackableLayer<TComponent>, ILayer<TComponent>
{
    IObservable<Guid> EntityCreated { get; }
    IObservable<Guid> EntityDisposed { get; }
    void ClearEntities();
}

public interface IReadOnlyDataLayer<in TComponent> : IReadOnlyLayer<TComponent>
{
    IReadOnlyEntity<TComponent> GetReadOnlyEntity<UComponent>()
        where UComponent : TComponent;
    bool CheckSupported(Type componentType);

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
}

public interface IDataLayer<in TComponent>
    : IReadOnlyDataLayer<TComponent>, ILayer<TComponent>
{
    IEntity<TComponent> GetEntity<UComponent>()
        where UComponent : TComponent;

    ref UComponent InspectRaw<UComponent>(Guid id)
        where UComponent : TComponent;
    ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent;

    ref UComponent Require<UComponent>(Guid id)
        where UComponent : TComponent;
    ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent;

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

    ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent;
    ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent;

    IEnumerable<object> GetAll(Guid id);
    void Clear(Guid id);
    void Clear();
}

public interface IReadOnlyMonoDataLayer<in TComponent, TStoredComponent> : IReadOnlyDataLayer<TComponent>
    where TStoredComponent : TComponent
{
    IReadOnlyEntity<TStoredComponent> GetReadOnlyEntity();

    bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    ref readonly TStoredComponent Inspect(Guid id);
    ref readonly TStoredComponent InspectAny();
    bool Contains(Guid id);
    bool ContainsAny();

    Guid? Singleton();
}

public interface IMonoDataLayer<in TComponent, TStoredComponent>
    : IReadOnlyMonoDataLayer<TComponent, TStoredComponent>, IDataLayer<TComponent>
    where TStoredComponent : TComponent
{
    IEntity<TStoredComponent> GetEntity();

    ref TStoredComponent InspectRaw(Guid id);
    ref TStoredComponent InspectAnyRaw();

    ref TStoredComponent Require(Guid id);
    ref TStoredComponent RequireAny();

    ref TStoredComponent Acquire(Guid id);
    ref TStoredComponent Acquire(Guid id, out bool exists);
    ref TStoredComponent AcquireAny();
    ref TStoredComponent AcquireAny(out bool exists);
    ref TStoredComponent AcquireRaw(Guid id);
    ref TStoredComponent AcquireRaw(Guid id, out bool exists);

    bool Remove(Guid id);
    bool RemoveAny();
    bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component);
    bool RemoveAny([MaybeNullWhen(false)] out TStoredComponent component);

    ref TStoredComponent Set(Guid id, in TStoredComponent component);
    ref TStoredComponent SetAny(in TStoredComponent component);
}

public interface IReadOnlyTrackableDataLayer<in TComponent>
    : IReadOnlyDataLayer<TComponent>, IReadOnlyTrackableLayer<TComponent>
{
}

public interface ITrackableDataLayer<in TComponent>
    : IReadOnlyTrackableDataLayer<TComponent>, IDataLayer<TComponent>, ITrackableLayer<TComponent>
{
}

public interface IDataLayerTree<in TComponent>
{
    bool IsSublayerCachable { get; }
    bool CheckSupported(Type componentType);
    IDataLayer<TComponent>? FindTerminalDataLayer<UComponent>()
        where UComponent : TComponent;
}

public interface IReadOnlyCompositeLayer<in TComponent, out TSublayer>
    : IReadOnlyLayer<TComponent>, IDataLayerTree<TComponent>
    where TSublayer : IReadOnlyLayer<TComponent>
{
    IReadOnlyList<TSublayer> Sublayers { get; }
    T? GetSublayer<T>();
    T? GetSublayerRecursively<T>();
    IEnumerable<T> GetSublayers<T>();
    IEnumerable<T> GetSublayersRecursively<T>();
}

public interface ICompositeLayer<in TComponent, out TSublayer>
    : IReadOnlyCompositeLayer<TComponent, TSublayer>, ILayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
}

public interface ICompositeLayer<in TComponent>
    : ICompositeLayer<TComponent, ILayer<TComponent>>
{
}

public interface IReadOnlyCompositeDataLayer<in TComponent, out TSublayer>
    : IReadOnlyCompositeLayer<TComponent, TSublayer>, IReadOnlyDataLayer<TComponent>
    where TSublayer : IReadOnlyLayer<TComponent>
{
}

public interface ICompositeDataLayer<in TComponent, out TSublayer>
    : IReadOnlyCompositeDataLayer<TComponent, TSublayer>, ICompositeLayer<TComponent, TSublayer>, IDataLayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
}

public interface IDynamicCompositeLayer<in TComponent, TSublayer> : ICompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    IObservable<TSublayer> SublayerAdded { get; }
    IObservable<TSublayer> SublayerRemoved { get; }

    bool AddSublayer(TSublayer sublayer);
    bool ContainsSublayer(TSublayer sublayer);
    bool RemoveSublayer(TSublayer sublayer);
    void ClearSublayers();
}

public interface IDynamicCompositeLayer<TComponent> : IDynamicCompositeLayer<TComponent, ILayer<TComponent>>
{
}

public interface IParentLayerListener<in TComponent, in TParentLayer>
    where TParentLayer : ILayer<TComponent>
{
    void OnLayerAdded(TParentLayer parent);
    void OnLayerRemoved(TParentLayer parent);
}

public interface IParentLayerListener<TComponent> : IParentLayerListener<TComponent, ILayer<TComponent>>
{
}

public static class LayerExtensions
{
    public static IEntity<TComponent> CreateEntity<TComponent>(this ILayer<TComponent> layer)
        => layer.GetEntity(Guid.NewGuid());
    public static IEntity<TComponent> GetEntity<TComponent>(this ILayer<TComponent> layer, string id)
        => layer.GetEntity(Guid.Parse(id));
}
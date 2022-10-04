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

    bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    ref readonly UComponent Inspect<UComponent>(Guid entityId)
        where UComponent : TComponent;
    ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent;
    bool Contains<UComponent>(Guid entityId)
        where UComponent : TComponent;
    bool ContainsAny<UComponent>()
        where UComponent : TComponent;

    Guid Singleton<UComponent>()
        where UComponent : TComponent;
    IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;
    IEnumerable<Guid> Query();
}

public interface IDataLayer<in TComponent>
    : IReadOnlyDataLayer<TComponent>, ILayer<TComponent>
{
    IEntity<TComponent> GetEntity<UComponent>()
        where UComponent : TComponent;

    ref UComponent UnsafeInspect<UComponent>(Guid entityId)
        where UComponent : TComponent;
    ref UComponent UnsafeInspectAny<UComponent>()
        where UComponent : TComponent;

    ref UComponent Require<UComponent>(Guid entityId)
        where UComponent : TComponent;
    ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent;

    ref UComponent Acquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new();
    ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent, new();

    ref UComponent UnsafeAcquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new();
    ref UComponent UnsafeAcquire<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent, new();

    bool Remove<UComponent>(Guid entityId)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>()
        where UComponent : TComponent;
    bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    bool RemoveAny<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    void RemoveAll<UComponent>()
        where UComponent : TComponent;

    ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
        where UComponent : TComponent;
    ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent;

    IEnumerable<object> GetAll(Guid entityId);
    void Clear(Guid entityId);
    void Clear();
}

public interface IReadOnlyMonoDataLayer<in TComponent, TStoredComponent> : IReadOnlyDataLayer<TComponent>
    where TStoredComponent : TComponent
{
    IReadOnlyEntity<TStoredComponent> GetReadOnlyEntity();

    bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component);
    ref readonly TStoredComponent Inspect(Guid entityId);
    ref readonly TStoredComponent InspectAny();
    bool Contains(Guid entityId);
    bool ContainsAny();

    Guid Singleton();
}

public interface IMonoDataLayer<in TComponent, TStoredComponent>
    : IReadOnlyMonoDataLayer<TComponent, TStoredComponent>, IDataLayer<TComponent>
    where TStoredComponent : TComponent
{
    IEntity<TStoredComponent> GetEntity();

    ref TStoredComponent UnsafeInspect(Guid entityId);
    ref TStoredComponent UnsafeInspectAny();

    ref TStoredComponent Require(Guid entityId);
    ref TStoredComponent RequireAny();

    ref TStoredComponent Acquire(Guid entityId);
    ref TStoredComponent Acquire(Guid entityId, out bool exists);
    ref TStoredComponent UnsafeAcquire(Guid entityId);
    ref TStoredComponent UnsafeAcquire(Guid entityId, out bool exists);

    bool Remove(Guid entityId);
    bool RemoveAny();
    bool Remove(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component);
    bool RemoveAny([MaybeNullWhen(false)] out TStoredComponent component);

    ref TStoredComponent Set(Guid entityId, in TStoredComponent component);
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

public interface IReadOnlyCompositeLayer<in TComponent, out TSublayer> : IReadOnlyLayer<TComponent>
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
    bool IsSublayerCachable { get; }
    IDataLayer<TComponent>? FindTerminalDataLayer<UComponent>()
        where UComponent : TComponent;
    IDataLayer<TComponent> RequireTerminalDataLayer<UComponent>()
        where UComponent : TComponent;
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

public interface IParentLayerListener<in TComponent, in TParentLayer>
    where TParentLayer : ILayer<TComponent>
{
    void OnLayerAdded(TParentLayer parent);
    void OnLayerRemoved(TParentLayer parent);
}

public interface IParentLayerListener<in TParentLayer> : IParentLayerListener<IComponent, TParentLayer>
    where TParentLayer : ILayer<IComponent>
{
}

public static class LayerExtensions
{
    public static IEntity<TComponent> CreateEntity<TComponent>(this ILayer<TComponent> layer)
        => layer.GetEntity(Guid.NewGuid());
    public static IEntity<TComponent> GetEntity<TComponent>(this ILayer<TComponent> layer, string id)
        => layer.GetEntity(Guid.Parse(id));
}
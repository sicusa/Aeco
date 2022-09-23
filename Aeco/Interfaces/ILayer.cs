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
    bool Contains<UComponent>(Guid entityId)
        where UComponent : TComponent;

    Guid Singleton<UComponent>()
        where UComponent : TComponent;
    IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;
}

public interface IDataLayer<in TComponent>
    : IReadOnlyDataLayer<TComponent>, ILayer<TComponent>
{
    IEntity<TComponent> GetEntity<UComponent>()
        where UComponent : TComponent;

    ref UComponent Require<UComponent>(Guid entityId)
        where UComponent : TComponent;
    ref UComponent Acquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new();
    ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent, new();
    bool Remove<UComponent>(Guid entityId)
        where UComponent : TComponent;
    bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    void Set<UComponent>(Guid entityId, in UComponent component)
        where UComponent : TComponent;
    IEnumerable<object> GetAll(Guid entityId);
    void Clear(Guid entityId);
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
    IEnumerable<T> GetSublayers<T>();
}

public interface ICompositeLayer<in TComponent, out TSublayer>
    : IReadOnlyCompositeLayer<TComponent, TSublayer>, ILayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
}

public interface IReadOnlyCompositeDataLayer<in TComponent, out TSublayer>
    : IReadOnlyCompositeLayer<TComponent, TSublayer>, IReadOnlyDataLayer<TComponent>
    where TSublayer : IReadOnlyLayer<TComponent>
{
    bool IsTerminalDataLayer { get; }
    IDataLayer<TComponent>? FindTerminalDataLayer<UComponent>()
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
namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface ILayer<in TComponent>
{
    IEntity<TComponent> GetEntity(Guid id);
}

public interface ITrackableLayer<in TComponent> : ILayer<TComponent>
{
    IEnumerable<Guid> Entities { get; }
    IObservable<Guid> EntityCreated { get; }
    IObservable<Guid> EntityDisposed { get; }

    bool ContainsEntity(Guid id);
    void ClearEntities();
}

public interface IDataLayer<in TComponent> : ILayer<TComponent>
{
    bool CheckSupported(Type componentType);

    bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    ref UComponent Require<UComponent>(Guid entityId)
        where UComponent : TComponent;
    ref UComponent Acquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new();
    bool Contains<UComponent>(Guid entityId)
        where UComponent : TComponent;
    bool Remove<UComponent>(Guid entityId)
        where UComponent : TComponent;
    void Set<UComponent>(Guid entityId, in UComponent component)
        where UComponent : TComponent;
    void Clear(Guid entityId);

    Guid Singleton<UComponent>()
        where UComponent : TComponent;
    IEntity<TComponent> GetEntity<UComponent>()
        where UComponent : TComponent;
    IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;
    IEnumerable<object> GetAll(Guid entityId);
}

public interface ITrackableDataLayer<in TComponent> : IDataLayer<TComponent>, ITrackableLayer<TComponent>
{
}

public interface ICompositeLayer<in TComponent, out TSublayer> : ILayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
    IReadOnlyList<TSublayer> Sublayers { get; }
    T? GetSublayer<T>();
    IEnumerable<T> GetSublayers<T>();
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

public interface IDependentLayer<in TComponent, out TLayer> : ILayer<TComponent>
    where TLayer : ILayer<TComponent>
{
    IEnumerable<TLayer> Dependencies { get; }
}

public interface IParentLayerListener<in TComponent, in TParentLayer>
    where TParentLayer : ILayer<TComponent>
{
    void OnLayerAdded(TParentLayer parent);
    void OnLayerRemoved(TParentLayer parent);
}

public interface IParentLayerListener<in TParentLayer> : IParentLayerListener<object, TParentLayer>
    where TParentLayer : ILayer<object>
{
}

public static class LayerExtensions
{
    public static IEntity<TComponent> CreateEntity<TComponent>(this ILayer<TComponent> layer)
        => layer.GetEntity(Guid.NewGuid());
    public static IEntity<TComponent> GetEntity<TComponent>(this ILayer<TComponent> layer, string id)
        => layer.GetEntity(Guid.Parse(id));
}
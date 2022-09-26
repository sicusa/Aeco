namespace Aeco.Local;

using System.Reactive.Subjects;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

public class CompositeLayer<TComponent, TSublayer>
    : LocalDataLayerBase<TComponent, TComponent>, ICompositeDataLayer<TComponent, TSublayer>
    , ITrackableDataLayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
    public virtual bool IsTerminalDataLayer => false;

    public IEnumerable<Guid> Entities => _entities.Keys;
    public IObservable<Guid> EntityCreated => EntityCreatedSubject;
    public IObservable<Guid> EntityDisposed => EntityDisposedSubject;

    public IReadOnlyList<TSublayer> Sublayers => _sublayerList;
    protected IReadOnlySet<TSublayer> SublayerSet => _sublayerSet;

    protected Subject<Guid> EntityCreatedSubject { get; } = new();
    protected Subject<Guid> EntityDisposedSubject { get; } = new();

    private Dictionary<Guid, IEntity<TComponent>> _entities = new();

    private ImmutableHashSet<TSublayer> _sublayerSet = ImmutableHashSet<TSublayer>.Empty;
    private ImmutableList<TSublayer> _sublayerList = ImmutableList<TSublayer>.Empty;

    private ImmutableDictionary<IDataLayer<TComponent>, ImmutableHashSet<Type>> _dataLayers =
        ImmutableDictionary<IDataLayer<TComponent>, ImmutableHashSet<Type>>.Empty;
    private ImmutableDictionary<Type, IDataLayer<TComponent>> _dataLayerCache =
        ImmutableDictionary<Type, IDataLayer<TComponent>>.Empty;
    
    public CompositeLayer(params TSublayer[] sublayers)
    {
        foreach (var sublayer in sublayers) {
            RawAddSublayer(sublayer);
        }
    }

    public override bool CheckSupported(Type componentType)
        => true;

    protected override IEntity<TComponent> CreateEntity(Guid id)
    {
        var entity = base.CreateEntity(id);
        _entities[entity.Id] = entity;
        EntityCreatedSubject.OnNext(id);
        entity.Disposed.Subscribe(ReleaseEntity);
        return entity;
    }

    private void ReleaseEntity(IEntity<TComponent> entity)
    {
        _entities.Remove(entity.Id);
        EntityDisposedSubject.OnNext(entity.Id);
    }

    public virtual bool ContainsEntity(Guid id)
        => _entities.ContainsKey(id);

    public virtual void ClearEntities()
    {
        foreach (var (_, entity) in _entities) {
            entity.Dispose();
        }
    }

    protected bool RawAddSublayer(TSublayer sublayer)
    {
        var changed = ImmutableInterlocked.Update(
            ref _sublayerSet, (sublayers, sublayer) => sublayers.Add(sublayer), sublayer);
        if (!changed) { return false; }

        ImmutableInterlocked.Update(
            ref _sublayerList, (sublayers, sublayer) => sublayers.Add(sublayer), sublayer);

        if (sublayer is IDataLayer<TComponent> dataLayer) {
            ImmutableInterlocked.TryAdd(
                ref _dataLayers, dataLayer, ImmutableHashSet<Type>.Empty);
        }

        (sublayer as IParentLayerListener<TComponent, CompositeLayer<TComponent, TSublayer>>)?.OnLayerAdded(this);
        return true;
    }

    protected bool RawRemoveSublayer(TSublayer sublayer)
    {
        var changed = ImmutableInterlocked.Update(
            ref _sublayerSet, (sublayers, sublayer) => sublayers.Remove(sublayer), sublayer);
        if (!changed) { return false; }

        ImmutableInterlocked.Update(
            ref _sublayerList, (sublayers, sublayer) => sublayers.Remove(sublayer), sublayer);
        
        if (sublayer is IDataLayer<TComponent> dataLayer) {
            if (ImmutableInterlocked.TryRemove(ref _dataLayers, dataLayer, out var cachedComps)) {
                ImmutableInterlocked.Update(ref _dataLayerCache,
                    (cache, comps) => cache.RemoveRange(comps), cachedComps);
            }
        }

        (sublayer as IParentLayerListener<TComponent, CompositeLayer<TComponent, TSublayer>>)?.OnLayerRemoved(this);
        return true;
    }

    protected IReadOnlyList<TSublayer> RawClearSublayers()
    {
        var oldSublayers = _sublayerList;

        ImmutableInterlocked.Update(ref _sublayerSet, c => c.Clear());
        ImmutableInterlocked.Update(ref _sublayerList, c => c.Clear());
        ImmutableInterlocked.Update(ref _dataLayers, c => c.Clear());
        ImmutableInterlocked.Update(ref _dataLayerCache, c => c.Clear());

        return oldSublayers;
    }
    
    public virtual T? GetSublayer<T>()
    {
        foreach (var sublayer in _sublayerList) {
            if (sublayer is T result) {
                return result;
            }
        }
        return default(T);
    }

    public virtual IEnumerable<T> GetSublayers<T>()
    {
        foreach (var sublayer in _sublayerList) {
            if (sublayer is T result) {
                yield return result;
            }
        }
    }

    public IDataLayer<TComponent>? FindTerminalDataLayer<UComponent>()
        where UComponent : TComponent
    {
        var compT = typeof(UComponent);
        if (_dataLayerCache.TryGetValue(compT, out var cachedLayer)) {
            return cachedLayer;
        }
        foreach (var sublayer in Sublayers) {
            if (sublayer is not IDataLayer<TComponent> dataLayer
                    || !dataLayer.CheckSupported(compT)) {
                continue;
            }
            if (sublayer is ICompositeDataLayer<TComponent, TSublayer> compositeDataLayer
                    && !compositeDataLayer.IsTerminalDataLayer) {
                var terminalDataLayer = compositeDataLayer.FindTerminalDataLayer<UComponent>();
                if (terminalDataLayer == null) {
                    continue;
                }
                dataLayer = terminalDataLayer;
            }
            ImmutableInterlocked.AddOrUpdate(ref _dataLayers, dataLayer,
                _ => ImmutableHashSet<Type>.Empty, (_, comps) => comps.Add(compT));
            ImmutableInterlocked.TryAdd(ref _dataLayerCache, compT, dataLayer);
            return dataLayer;
        }
        return null;
    }

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        if (dataLayer == null) {
            component = default(UComponent);
            return false;
        }
        return dataLayer.TryGet<UComponent>(entityId, out component);
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable data layer for specified component");
        return ref dataLayer.Require<UComponent>(entityId);
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable data layer for specified component");
        return ref dataLayer.Acquire<UComponent>(entityId);
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable data layer for specified component");
        return ref dataLayer.Acquire<UComponent>(entityId, out exists);
    }

    public override bool Contains<UComponent>(Guid entityId)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        return dataLayer != null ? dataLayer.Contains<UComponent>(entityId) : false;
    }

    public override void Set<UComponent>(Guid entityId, in UComponent component)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable data layer for specified component");
        dataLayer.Set(entityId, component);
    }

    public override IEnumerable<object> GetAll(Guid entityId)
        => _dataLayers.SelectMany(s => s.Key.GetAll(entityId));

    public override Guid Singleton<UComponent>()
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        if (dataLayer == null) {
            throw new KeyNotFoundException("Singleton not found: " + typeof(UComponent));
        }
        return dataLayer.Singleton<UComponent>();
    }

    public override IEnumerable<Guid> Query<UComponent>()
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        if (dataLayer == null) {
            return Enumerable.Empty<Guid>();
        }
        return dataLayer.Query<UComponent>();
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        if (dataLayer == null) {
            return false;
        }
        return dataLayer.Remove<UComponent>(entityId);
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        if (dataLayer == null) {
            component = default(UComponent);
            return false;
        }
        return dataLayer.Remove<UComponent>(entityId, out component);
    }

    public override void Clear(Guid entityId)
    {
        foreach (var (dataLayer, _) in _dataLayers) {
            dataLayer.Clear(entityId);
        }
    }

    public override void Clear()
    {
        foreach (var (dataLayer, _) in _dataLayers) {
            dataLayer.Clear();
        }
    }
}

public class CompositeLayer<TComponent> : CompositeLayer<TComponent, ILayer<TComponent>>
{
    public CompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class CompositeLayer : CompositeLayer<IComponent>
{
    public CompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
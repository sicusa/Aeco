namespace Aeco.Local;

using System.Diagnostics.CodeAnalysis;

public class CompositeLayer<TComponent, TSublayer>
    : LocalDataLayerBase<TComponent, TComponent>
    , ICompositeDataLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public virtual bool IsProvidedLayerCachable => true;

    public IReadOnlyList<TSublayer> Sublayers => _sublayers;

    private HashSet<TSublayer> _sublayerSet = new();
    private List<TSublayer> _sublayers = new();

    private Dictionary<ILayer<TComponent>, SparseSet<Type>> _dataLayers = new();
    private SparseSet<ILayer<TComponent>> _dataLayerCache = new();

    public CompositeLayer()
    {
    }
    
    public CompositeLayer(params TSublayer[] sublayers)
    {
        InternalAddSublayers(sublayers);
    }

    public override bool CheckSupported(Type componentType)
        => true;

    protected bool InternalAddSublayer(TSublayer sublayer)
    {
        if (!_sublayerSet.Add(sublayer)) {
            return false;
        }
        _sublayers.Add(sublayer);

        if (sublayer is IExpandableDataLayer<TComponent> dataLayer) {
            _dataLayers.Add(dataLayer, new());
        }
        (sublayer as IParentLayerListener<TComponent, CompositeLayer<TComponent, TSublayer>>)?.OnLayerAdded(this);
        return true;
    }

    protected void InternalAddSublayers(params TSublayer[] sublayers)
    {
        foreach (var sublayer in sublayers) {
            InternalAddSublayer(sublayer);
        }
    }

    protected bool InternalContainsSublayer(TSublayer sublayer)
        => _sublayers.Contains(sublayer);

    protected bool InternalRemoveSublayer(TSublayer sublayer)
    {
        if (!_sublayerSet.Remove(sublayer)) {
            return false;
        }
        _sublayers.Remove(sublayer);
        
        if (sublayer is IExpandableDataLayer<TComponent> dataLayer
                && _dataLayers.Remove(dataLayer, out var cachedComps)) {
            foreach (var index in cachedComps.AsKeySpan()) {
                _dataLayerCache.Remove(index);
            }
        }

        (sublayer as IParentLayerListener<TComponent, CompositeLayer<TComponent, TSublayer>>)?.OnLayerRemoved(this);
        return true;
    }

    protected void InternalClearSublayers()
    {
        _sublayers.Clear();
        _sublayerSet.Clear();
        _dataLayers.Clear();
        _dataLayerCache.Clear();
    }
    
    public virtual T? GetSublayer<T>()
    {
        foreach (var sublayer in _sublayers) {
            if (sublayer is T result) {
                return result;
            }
        }
        return default;
    }

    public virtual T? GetSublayerRecursively<T>()
    {
        foreach (var sublayer in _sublayers) {
            if (sublayer is T result) {
                return result;
            }
            if (sublayer is ICompositeLayer<TComponent, TSublayer> compositeLayer) {
                var sublayerRes = compositeLayer.GetSublayerRecursively<T>();
                if (sublayerRes != null) {
                    return sublayerRes;
                }
            }
        }
        return default;
    }

    public virtual IEnumerable<T> GetSublayers<T>()
    {
        foreach (var sublayer in _sublayers) {
            if (sublayer is T result) {
                yield return result;
            }
        }
    }

    public virtual IEnumerable<T> GetSublayersRecursively<T>()
    {
        foreach (var sublayer in _sublayers) {
            if (sublayer is T result) {
                yield return result;
            }
            if (sublayer is ICompositeLayer<TComponent, TSublayer> compositeLayer) {
                foreach (var sublayerRes in compositeLayer.GetSublayersRecursively<T>()) {
                    yield return sublayerRes;
                }
            }
        }
    }

    public override bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        if (dataLayer == null) {
            component = default;
            return false;
        }
        return dataLayer.TryGet<UComponent>(id, out component);
    }

    public override ref readonly UComponent Inspect<UComponent>(Guid id)
    {
        var dataLayer = RequireReadableDataLayer<UComponent>();
        return ref dataLayer.Inspect<UComponent>(id);
    }

    public override ref UComponent InspectRaw<UComponent>(Guid id)
    {
        var dataLayer = RequireWritableDataLayer<UComponent>();
        return ref dataLayer.InspectRaw<UComponent>(id);
    }

    public override ref UComponent Require<UComponent>(Guid id)
    {
        var dataLayer = RequireWritableDataLayer<UComponent>();
        return ref dataLayer.Require<UComponent>(id);
    }

    public override ref UComponent Acquire<UComponent>(Guid id)
    {
        var dataLayer = RequireExpandableDataLayer<UComponent>();
        return ref dataLayer.Acquire<UComponent>(id);
    }

    public override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
    {
        var dataLayer = RequireExpandableDataLayer<UComponent>();
        return ref dataLayer.Acquire<UComponent>(id, out exists);
    }

    public override ref UComponent AcquireRaw<UComponent>(Guid id)
    {
        var dataLayer = RequireExpandableDataLayer<UComponent>();
        return ref dataLayer.AcquireRaw<UComponent>(id);
    }

    public override ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
    {
        var dataLayer = RequireExpandableDataLayer<UComponent>();
        return ref dataLayer.AcquireRaw<UComponent>(id, out exists);
    }

    public override bool Contains<UComponent>(Guid id)
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        return dataLayer != null ? dataLayer.Contains<UComponent>(id) : false;
    }

    public override bool ContainsAny<UComponent>()
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        return dataLayer != null ? dataLayer.ContainsAny<UComponent>() : false;
    }

    public override ref UComponent Set<UComponent>(Guid id, in UComponent component)
        => ref RequireSettableDataLayer<UComponent>().Set(id, component);

    public override IEnumerable<object> GetAll(Guid id)
        => _dataLayers.Keys.OfType<IReadableDataLayer<TComponent>>()
            .SelectMany(s => s.GetAll(id));

    public override Guid? Singleton<UComponent>()
        => GetReadableDataLayer<UComponent>()?.Singleton<UComponent>();

    public override int GetCount()
        => _dataLayers.Keys.Sum(l =>
            l is IReadableDataLayer<TComponent> readable
                ? readable.GetCount() : 0);

    public override int GetCount<UComponent>()
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        return dataLayer != null ? dataLayer.GetCount<UComponent>() : 0;
    }

    public override IEnumerable<Guid> Query<UComponent>()
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        if (dataLayer == null) {
            return Enumerable.Empty<Guid>();
        }
        return dataLayer.Query<UComponent>();
    }

    public override IEnumerable<Guid> Query()
        => QueryUtil.Union(
            _dataLayers.Keys.OfType<IReadableDataLayer<TComponent>>()
                .Select(l => l.Query()));

    public override bool Remove<UComponent>(Guid id)
    {
        var dataLayer = GetShrinkableDataLayer<UComponent>();
        if (dataLayer == null) {
            return false;
        }
        return dataLayer.Remove<UComponent>(id);
    }

    public override bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = GetShrinkableDataLayer<UComponent>();
        if (dataLayer == null) {
            component = default;
            return false;
        }
        return dataLayer.Remove<UComponent>(id, out component);
    }

    public override void RemoveAll<UComponent>()
        => GetShrinkableDataLayer<UComponent>()?.RemoveAll<UComponent>();

    public override void Clear(Guid id)
    {
        foreach (var (dataLayer, _) in _dataLayers) {
            (dataLayer as IShrinkableDataLayer<TComponent>)?.Clear(id);
        }
    }

    public override void Clear()
    {
        foreach (var (dataLayer, _) in _dataLayers) {
            (dataLayer as IShrinkableDataLayer<TComponent>)?.Clear();
        }
    }

    IReadableDataLayer<TComponent>? ILayerProvider<TComponent, IReadableDataLayer<TComponent>>.GetLayer<UComponent>()
        => GetReadableDataLayer<UComponent>();
    IWritableDataLayer<TComponent>? ILayerProvider<TComponent, IWritableDataLayer<TComponent>>.GetLayer<UComponent>()
        => GetWritableDataLayer<UComponent>();
    IExpandableDataLayer<TComponent>? ILayerProvider<TComponent, IExpandableDataLayer<TComponent>>.GetLayer<UComponent>()
        => GetExpandableDataLayer<UComponent>();
    ISettableDataLayer<TComponent>? ILayerProvider<TComponent, ISettableDataLayer<TComponent>>.GetLayer<UComponent>()
        => GetSettableDataLayer<UComponent>();
    IShrinkableDataLayer<TComponent>? ILayerProvider<TComponent, IShrinkableDataLayer<TComponent>>.GetLayer<UComponent>()
        => GetShrinkableDataLayer<UComponent>();
    IDataLayer<TComponent>? ILayerProvider<TComponent, IDataLayer<TComponent>>.GetLayer<UComponent>()
        => GetDataLayer<UComponent>();

    public TDataLayer? GenericGetDataLayer<UComponent, TDataLayer>()
        where UComponent : TComponent
        where TDataLayer : class, IDataLayerBase<TComponent>
    {
        var compIndex = TypeIndexer<UComponent>.Index;
        if (_dataLayerCache.TryGetValue(compIndex, out var cachedLayer)) {
            return cachedLayer as TDataLayer;
        }

        var type = typeof(UComponent);
        foreach (var sublayer in _sublayers) {
            TDataLayer dataLayer;
            TDataLayer? terminalDataLayer;
            if (sublayer is ILayerProvider<TComponent, TDataLayer> provider) {
                if (!provider.CheckSupported(type)) { continue; }
                terminalDataLayer = provider.GetLayer<UComponent>();
                if (terminalDataLayer == null) {
                    continue;
                }
                dataLayer = (TDataLayer)provider;
                terminalDataLayer = provider.IsProvidedLayerCachable
                    ? terminalDataLayer : dataLayer;
            }
            else {
                terminalDataLayer = sublayer as TDataLayer;
                if (terminalDataLayer == null || !terminalDataLayer.CheckSupported(typeof(UComponent))) {
                    continue;
                }
                dataLayer = terminalDataLayer;
            }
            if (!_dataLayers.TryGetValue(dataLayer, out var cachedComps)) {
                cachedComps = new SparseSet<Type>();
                _dataLayers.Add(dataLayer, cachedComps);
            }
            cachedComps.Add(compIndex, type);
            _dataLayerCache.Add(compIndex, terminalDataLayer);
            return terminalDataLayer;
        }
        return null;
    }

    public IReadableDataLayer<TComponent>? GetReadableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IReadableDataLayer<TComponent>>();

    public IWritableDataLayer<TComponent>? GetWritableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IWritableDataLayer<TComponent>>();

    public IExpandableDataLayer<TComponent>? GetExpandableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IExpandableDataLayer<TComponent>>();

    public ISettableDataLayer<TComponent>? GetSettableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, ISettableDataLayer<TComponent>>();

    public IShrinkableDataLayer<TComponent>? GetShrinkableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IShrinkableDataLayer<TComponent>>();

    public IDataLayer<TComponent>? GetDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IDataLayer<TComponent>>();

    public IReadableDataLayer<TComponent> RequireReadableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetReadableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable readable data layer for component: " + typeof(UComponent));

    public IWritableDataLayer<TComponent> RequireWritableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetWritableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable writable data layer for component: " + typeof(UComponent));

    public IExpandableDataLayer<TComponent> RequireExpandableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetExpandableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable expandable data layer for component: " + typeof(UComponent));

    public ISettableDataLayer<TComponent> RequireSettableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetSettableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable settable data layer for component: " + typeof(UComponent));

    public IShrinkableDataLayer<TComponent> RequireShrinkableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetShrinkableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable shrinkable data layer for component: " + typeof(UComponent));

    public IDataLayer<TComponent> RequireDataLayer<UComponent>()
        where UComponent : TComponent
        => GetDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable data layer for component: " + typeof(UComponent));
}

public class CompositeLayer<TComponent>
    : CompositeLayer<TComponent, ILayer<TComponent>>, ICompositeLayer<TComponent>
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
namespace Aeco.Local;

using System.Diagnostics.CodeAnalysis;

public class CompositeLayer<TComponent, TSublayer>
    : DataLayerBase<TComponent, TComponent>
    , ICompositeDataLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public virtual bool IsProvidedLayerCachable => true;

    public IReadOnlyList<TSublayer> Sublayers => _sublayers;

    private HashSet<TSublayer> _sublayerSet = new();
    private List<TSublayer> _sublayers = new();

    private Dictionary<IBasicDataLayer<TComponent>, SparseSet<Type>> _dataLayers = new();
    private SparseSet<IBasicDataLayer<TComponent>> _dataLayerCache = new();

    public CompositeLayer()
    {
    }
    
    public CompositeLayer(params TSublayer[] sublayers)
    {
        InternalAddSublayers(sublayers);
    }

    public override bool CheckComponentSupported(Type componentType)
        => true;

    protected bool InternalAddSublayer(TSublayer sublayer)
    {
        if (!_sublayerSet.Add(sublayer)) {
            return false;
        }
        _sublayers.Add(sublayer);

        if (sublayer is IBasicDataLayer<TComponent> dataLayer) {
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
        
        if (sublayer is IBasicDataLayer<TComponent> dataLayer
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

    public override bool Contains<UComponent>(uint id)
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        return dataLayer != null ? dataLayer.Contains<UComponent>(id) : false;
    }

    public override bool ContainsAny<UComponent>()
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        return dataLayer != null ? dataLayer.ContainsAny<UComponent>() : false;
    }

    public override uint? Singleton<UComponent>()
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

    public override IEnumerable<uint> Query<UComponent>()
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        if (dataLayer == null) {
            return Enumerable.Empty<uint>();
        }
        return dataLayer.Query<UComponent>();
    }

    public override IEnumerable<uint> Query()
        => QueryUtil.Union(
            _dataLayers.Keys.OfType<IReadableDataLayer<TComponent>>()
                .Select(l => l.Query()));

    public virtual bool TryGet<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        if (dataLayer == null) {
            component = default;
            return false;
        }
        return dataLayer.TryGet<UComponent>(id, out component);
    }

    public virtual ref readonly UComponent Inspect<UComponent>(uint id)
        where UComponent : TComponent
        => ref RequireReadableDataLayer<UComponent>().Inspect<UComponent>(id);

    public virtual ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent
        => ref RequireReadableDataLayer<UComponent>().InspectAny<UComponent>();

    public virtual ref readonly UComponent InspectOrNullRef<UComponent>(uint id)
        where UComponent : TComponent
        => ref RequireReadableDataLayer<UComponent>().InspectOrNullRef<UComponent>(id);

    public virtual ref readonly UComponent InspectAnyOrNullRef<UComponent>()
        where UComponent : TComponent
        => ref RequireReadableDataLayer<UComponent>().InspectAnyOrNullRef<UComponent>();

    public virtual ref UComponent InspectRaw<UComponent>(uint id)
        where UComponent : TComponent
        => ref RequireWritableDataLayer<UComponent>().InspectRaw<UComponent>(id);

    public virtual ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent
        => ref RequireWritableDataLayer<UComponent>().InspectAnyRaw<UComponent>();

    public virtual ref UComponent Require<UComponent>(uint id)
        where UComponent : TComponent
        => ref RequireWritableDataLayer<UComponent>().Require<UComponent>(id);

    public virtual ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent
        => ref RequireWritableDataLayer<UComponent>().RequireAny<UComponent>();

    public virtual ref UComponent RequireOrNullRef<UComponent>(uint id)
        where UComponent : TComponent
        => ref RequireWritableDataLayer<UComponent>().RequireOrNullRef<UComponent>(id);

    public virtual ref UComponent RequireAnyOrNullRef<UComponent>()
        where UComponent : TComponent
        => ref RequireWritableDataLayer<UComponent>().RequireAnyOrNullRef<UComponent>();

    public virtual ref UComponent Acquire<UComponent>(uint id)
        where UComponent : TComponent, new()
        => ref RequireExpandableDataLayer<UComponent>().Acquire<UComponent>(id);

    public virtual ref UComponent Acquire<UComponent>(uint id, out bool exists)
        where UComponent : TComponent, new()
        => ref RequireExpandableDataLayer<UComponent>().Acquire<UComponent>(id, out exists);

    public virtual ref UComponent AcquireAny<UComponent>()
        where UComponent : TComponent, new()
        => ref RequireExpandableDataLayer<UComponent>().AcquireAny<UComponent>();

    public virtual ref UComponent AcquireRaw<UComponent>(uint id)
        where UComponent : TComponent, new()
        => ref RequireExpandableDataLayer<UComponent>().AcquireRaw<UComponent>(id);

    public virtual ref UComponent AcquireRaw<UComponent>(uint id, out bool exists)
        where UComponent : TComponent, new()
        => ref RequireExpandableDataLayer<UComponent>().AcquireRaw<UComponent>(id, out exists);

    public virtual ref UComponent AcquireAnyRaw<UComponent>()
        where UComponent : TComponent, new()
        => ref RequireExpandableDataLayer<UComponent>().AcquireAnyRaw<UComponent>();
    
    public virtual ComponentRef<UComponent> GetRef<UComponent>(uint id)
        where UComponent : TComponent
        => RequireReferableDataLayer<UComponent>().GetRef<UComponent>(id);

    public virtual ref UComponent Set<UComponent>(uint id, in UComponent component)
        where UComponent : TComponent, new()
        => ref RequireSettableDataLayer<UComponent>().Set(id, component);

    public virtual IEnumerable<object> GetAll(uint id)
        => _dataLayers.Keys.OfType<IReadableDataLayer<TComponent>>()
            .SelectMany(s => s.GetAll(id));

    public virtual bool Remove<UComponent>(uint id)
        where UComponent : TComponent
    {
        var dataLayer = GetShrinkableDataLayer<UComponent>();
        if (dataLayer == null) {
            return false;
        }
        return dataLayer.Remove<UComponent>(id);
    }

    public virtual bool Remove<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var dataLayer = GetShrinkableDataLayer<UComponent>();
        if (dataLayer == null) {
            component = default;
            return false;
        }
        return dataLayer.Remove<UComponent>(id, out component);
    }

    public virtual void RemoveAll<UComponent>()
        where UComponent : TComponent
        => GetShrinkableDataLayer<UComponent>()?.RemoveAll<UComponent>();

    public virtual void Clear(uint id)
    {
        foreach (var (dataLayer, _) in _dataLayers) {
            (dataLayer as IShrinkableDataLayer<TComponent>)?.Clear(id);
        }
    }

    public virtual void Clear()
    {
        foreach (var (dataLayer, _) in _dataLayers) {
            (dataLayer as IShrinkableDataLayer<TComponent>)?.Clear();
        }
    }

    IReadableDataLayer<TComponent>? ILayerProvider<TComponent, IReadableDataLayer<TComponent>>.FindLayer<UComponent>()
        => GetReadableDataLayer<UComponent>();
    IWritableDataLayer<TComponent>? ILayerProvider<TComponent, IWritableDataLayer<TComponent>>.FindLayer<UComponent>()
        => GetWritableDataLayer<UComponent>();
    IExpandableDataLayer<TComponent>? ILayerProvider<TComponent, IExpandableDataLayer<TComponent>>.FindLayer<UComponent>()
        => GetExpandableDataLayer<UComponent>();
    ISettableDataLayer<TComponent>? ILayerProvider<TComponent, ISettableDataLayer<TComponent>>.FindLayer<UComponent>()
        => GetSettableDataLayer<UComponent>();
    IReferableDataLayer<TComponent>? ILayerProvider<TComponent, IReferableDataLayer<TComponent>>.FindLayer<UComponent>()
        => GetReferableDataLayer<UComponent>();
    IShrinkableDataLayer<TComponent>? ILayerProvider<TComponent, IShrinkableDataLayer<TComponent>>.FindLayer<UComponent>()
        => GetShrinkableDataLayer<UComponent>();

    private TDataLayer? GenericGetDataLayer<UComponent, TDataLayer>()
        where UComponent : TComponent
        where TDataLayer : class, IBasicDataLayer<TComponent>
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
                if (!provider.CheckComponentSupported(type)) { continue; }
                terminalDataLayer = provider.FindLayer<UComponent>();
                if (terminalDataLayer == null) {
                    continue;
                }
                dataLayer = (TDataLayer)provider;
                terminalDataLayer = provider.IsProvidedLayerCachable
                    ? terminalDataLayer : dataLayer;
            }
            else {
                terminalDataLayer = sublayer as TDataLayer;
                if (terminalDataLayer == null || !terminalDataLayer.CheckComponentSupported(typeof(UComponent))) {
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

    protected IReadableDataLayer<TComponent>? GetReadableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IReadableDataLayer<TComponent>>();

    protected IWritableDataLayer<TComponent>? GetWritableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IWritableDataLayer<TComponent>>();

    protected IExpandableDataLayer<TComponent>? GetExpandableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IExpandableDataLayer<TComponent>>();

    protected ISettableDataLayer<TComponent>? GetSettableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, ISettableDataLayer<TComponent>>();

    protected IReferableDataLayer<TComponent>? GetReferableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IReferableDataLayer<TComponent>>();

    protected IShrinkableDataLayer<TComponent>? GetShrinkableDataLayer<UComponent>()
        where UComponent : TComponent
        => GenericGetDataLayer<UComponent, IShrinkableDataLayer<TComponent>>();

    protected IReadableDataLayer<TComponent> RequireReadableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetReadableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable readable data layer for component: " + typeof(UComponent));

    protected IWritableDataLayer<TComponent> RequireWritableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetWritableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable writable data layer for component: " + typeof(UComponent));

    protected IExpandableDataLayer<TComponent> RequireExpandableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetExpandableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable expandable data layer for component: " + typeof(UComponent));

    protected ISettableDataLayer<TComponent> RequireSettableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetSettableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable settable data layer for component: " + typeof(UComponent));

    protected IReferableDataLayer<TComponent> RequireReferableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetReferableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable settable data layer for component: " + typeof(UComponent));

    protected IShrinkableDataLayer<TComponent> RequireShrinkableDataLayer<UComponent>()
        where UComponent : TComponent
        => GetShrinkableDataLayer<UComponent>()
            ?? throw new NotSupportedException("No suitable shrinkable data layer for component: " + typeof(UComponent));
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
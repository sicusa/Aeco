namespace Aeco.Concurrent;

using System.Reactive.Subjects;

public class ConcurrentDynamicCompositeLayer<TComponent, TSublayer> : ConcurrentCompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public IObservable<TSublayer> SublayerAdded => _sublayerAdded;
    public IObservable<TSublayer> SublayerRemoved => _sublayerRemoved;

    private Subject<TSublayer> _sublayerAdded = new();
    private Subject<TSublayer> _sublayerRemoved = new();

    public ConcurrentDynamicCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public bool AddSublayer(TSublayer sublayer)
    {
        if (!RawAddSublayer(sublayer)) {
            return false;
        }
        _sublayerAdded.OnNext(sublayer);
        return true;
    }

    public bool ContainsSublayer(TSublayer sublayer)
        => SublayerSet.Contains(sublayer);

    public bool RemoveSublayer(TSublayer sublayer)
    {
        if (!RawRemoveSublayer(sublayer)) {
            return false;
        }
        _sublayerRemoved.OnNext(sublayer);
        return true;
    }

    public void ClearSublayers()
    {
        foreach (var sublayer in RawClearSublayers()) {
            _sublayerRemoved.OnNext(sublayer);
        }
    }
}

public class ConcurrentDynamicCompositeLayer<TComponent> : ConcurrentDynamicCompositeLayer<TComponent, ILayer<TComponent>>
{
    public ConcurrentDynamicCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class ConcurrentDynamicCompositeLayer : ConcurrentDynamicCompositeLayer<IComponent>
{
    public ConcurrentDynamicCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
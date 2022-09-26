namespace Aeco.Reactive;

using System.Reactive.Subjects;

public class ReactiveDynamicCompositeLayer<TComponent, TSublayer> : ReactiveCompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public IObservable<TSublayer> SublayerAdded => _sublayerAdded;
    public IObservable<TSublayer> SublayerRemoved => _sublayerRemoved;

    private Subject<TSublayer> _sublayerAdded = new();
    private Subject<TSublayer> _sublayerRemoved = new();

    public ReactiveDynamicCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params TSublayer[] sublayers)
        : base(eventDataLayer, sublayers)
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

public class ReactiveDynamicCompositeLayer<TComponent> : ReactiveDynamicCompositeLayer<TComponent, ILayer<TComponent>>
{
    public ReactiveDynamicCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<TComponent>[] sublayers)
        : base(eventDataLayer, sublayers)
    {
    }
}

public class ReactiveDynamicCompositeLayer : ReactiveDynamicCompositeLayer<IComponent>
{
    public ReactiveDynamicCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<IComponent>[] sublayers)
        : base(eventDataLayer, sublayers)
    {
    }
}
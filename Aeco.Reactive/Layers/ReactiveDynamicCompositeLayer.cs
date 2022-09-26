namespace Aeco.Reactive;

using System.Reactive.Subjects;

public class ReactiveDynamicCompositeLayer<TSublayer> : ReactiveCompositeLayer<TSublayer>
    where TSublayer : ILayer<IComponent>
{
    public IObservable<TSublayer> SublayerAdded => _sublayerAdded;
    public IObservable<TSublayer> SublayerRemoved => _sublayerRemoved;

    private Subject<TSublayer> _sublayerAdded = new();
    private Subject<TSublayer> _sublayerRemoved = new();

    public ReactiveDynamicCompositeLayer(params TSublayer[] sublayers)
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

public class ReactiveDynamicCompositeLayer : ReactiveDynamicCompositeLayer<ILayer<IComponent>>
{
    public ReactiveDynamicCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
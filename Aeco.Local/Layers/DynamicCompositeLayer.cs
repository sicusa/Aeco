namespace Aeco.Local;

using System.Reactive.Subjects;

public class DynamicCompositeLayer<TComponent, TSublayer>
    : CompositeLayer<TComponent, TSublayer>, IDynamicCompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public IObservable<TSublayer> SublayerAdded => _sublayerAdded;
    public IObservable<TSublayer> SublayerRemoved => _sublayerRemoved;

    private Subject<TSublayer> _sublayerAdded = new();
    private Subject<TSublayer> _sublayerRemoved = new();

    public DynamicCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public bool AddSublayer(TSublayer sublayer)
    {
        if (!InternalAddSublayer(sublayer)) {
            return false;
        }
        _sublayerAdded.OnNext(sublayer);
        return true;
    }

    public bool ContainsSublayer(TSublayer sublayer)
        => SublayerSet.Contains(sublayer);

    public bool RemoveSublayer(TSublayer sublayer)
    {
        if (!InternalRemoveSublayer(sublayer)) {
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

public class DynamicCompositeLayer<TComponent> : DynamicCompositeLayer<TComponent, ILayer<TComponent>>
{
    public DynamicCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class DynamicCompositeLayer : DynamicCompositeLayer<IComponent>
{
    public DynamicCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
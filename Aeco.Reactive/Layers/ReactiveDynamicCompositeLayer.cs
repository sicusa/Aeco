namespace Aeco.Reactive;

using System.Reactive.Subjects;

public class ReactiveDynamicCompositeLayer<TComponent, TSublayer> : ReactiveCompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
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
        if (!InternalAddSublayer(sublayer)) {
            return false;
        }
        _sublayerAdded.OnNext(sublayer);
        return true;
    }

    public bool ContainsSublayer(TSublayer sublayer)
        => InternalContainsSublayer(sublayer);

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
        foreach (var sublayer in Sublayers) {
            _sublayerRemoved.OnNext(sublayer);
        }
        InternalClearSublayers();
    }
}

public class ReactiveDynamicCompositeLayer<TComponent>
    : ReactiveDynamicCompositeLayer<TComponent, ILayer<TComponent>>, IDynamicCompositeLayer<TComponent>
{
    public ReactiveDynamicCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class ReactiveDynamicCompositeLayer : ReactiveDynamicCompositeLayer<IComponent>
{
    public ReactiveDynamicCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
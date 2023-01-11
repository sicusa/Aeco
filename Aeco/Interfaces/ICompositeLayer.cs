namespace Aeco;

public interface ICompositeLayer<in TComponent, out TSublayer>
    : ILayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
    IReadOnlyList<TSublayer> Sublayers { get; }
    T? GetSublayer<T>();
    T? GetSublayerRecursively<T>();
    IEnumerable<T> GetSublayers<T>();
    IEnumerable<T> GetSublayersRecursively<T>();
}

public interface ICompositeLayer<in TComponent>
    : ICompositeLayer<TComponent, ILayer<TComponent>>
{
}

public interface IDynamicCompositeLayer<in TComponent, TSublayer>
    : ICompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    IObservable<TSublayer> SublayerAdded { get; }
    IObservable<TSublayer> SublayerRemoved { get; }

    bool AddSublayer(TSublayer sublayer);
    bool RemoveSublayer(TSublayer sublayer);
    bool ContainsSublayer(TSublayer sublayer);
    void ClearSublayers();
}

public interface IDynamicCompositeLayer<TComponent>
    : IDynamicCompositeLayer<TComponent, ILayer<TComponent>>
{
}
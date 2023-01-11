namespace Aeco;

public interface ICompositeDataLayer<in TComponent, out TSublayer>
    : ICompositeLayer<TComponent, TSublayer>, IDataLayer<TComponent>
    , ILayerProvider<TComponent, IReadableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IWritableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IExpandableDataLayer<TComponent>>
    , ILayerProvider<TComponent, ISettableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IShrinkableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IDataLayer<TComponent>>
    where TSublayer : ILayer<TComponent>
{
}
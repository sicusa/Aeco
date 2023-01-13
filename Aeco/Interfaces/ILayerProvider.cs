namespace Aeco;

public interface ILayerProvider<in TComponent, out TLayer>
    where TLayer : ILayer<TComponent>
{
    bool IsProvidedLayerCachable { get; }
    bool CheckComponentSupported(Type componentType);

    TLayer? FindLayer<UComponent>()
        where UComponent : TComponent;
}
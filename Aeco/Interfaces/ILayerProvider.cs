namespace Aeco;

public interface ILayerProvider<in TComponent, out TLayer>
    where TLayer : ILayer<TComponent>
{
    bool IsProvidedLayerCachable { get; }
    bool CheckSupported(Type componentType);

    TLayer? GetLayer<UComponent>()
        where UComponent : TComponent;
}
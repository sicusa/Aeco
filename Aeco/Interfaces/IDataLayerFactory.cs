namespace Aeco;

public interface IDataLayerFactory<in TComponent>
{
    IBasicDataLayer<TComponent> Create<TStoredComponent>()
        where TStoredComponent : TComponent, new();
}
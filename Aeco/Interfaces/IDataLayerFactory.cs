namespace Aeco;

public interface IDataLayerFactory<in TComponent>
{
    IDataLayer<TComponent> Create<TStoredComponent>()
        where TStoredComponent : TComponent, new();
}
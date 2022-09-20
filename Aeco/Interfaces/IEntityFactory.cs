namespace Aeco;

public interface IEntityFactory<TComponent, in TDataLayer>
    where TDataLayer : IDataLayer<TComponent>
{
    IEntity<TComponent> GetEntity(TDataLayer layer, Guid id);
}
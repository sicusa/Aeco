namespace Aeco.Persistence;

using Aeco.Serialization;

public interface IPersistenceLayer<TComponent, TSelectedComponent>
    : ILayer<TComponent>, IParentLayerListener<TComponent, ITrackableDataLayer<TComponent>>
    where TSelectedComponent : TComponent
{
    IEntitySerializer<TSelectedComponent> Serializer { get; }
}
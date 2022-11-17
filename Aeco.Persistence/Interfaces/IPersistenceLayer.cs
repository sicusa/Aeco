namespace Aeco.Persistence;

using Aeco.Serialization;

public interface IPersistenceLayer<TComponent, TSelectedComponent> : ILayer<TComponent>
    where TSelectedComponent : TComponent
{
    IEntitySerializer<TSelectedComponent> Serializer { get; }
}
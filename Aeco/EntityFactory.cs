namespace Aeco;

public class EntityFactory<TComponent, TDataLayer> : IEntityFactory<TComponent, TDataLayer>
    where TDataLayer : IDataLayer<TComponent>
{
    public uint PoolCapacity { get; } = 1000;

    private Stack<Entity<TComponent, TDataLayer>> _entityPool = new();

    public IEntity<TComponent> GetEntity(TDataLayer dataLayer, Guid id)
    {
        if (_entityPool.TryPop(out var entity)) {
            entity.Reset(dataLayer, id);
        }
        else {
            entity = new Entity<TComponent, TDataLayer>(dataLayer, id);
        }
        entity.Disposed.Subscribe(ReleaseEntity);
        return entity;
    }

    private void ReleaseEntity(IEntity<TComponent> entity)
    {
        if (_entityPool.Count < PoolCapacity) {
            _entityPool.Push((Entity<TComponent, TDataLayer>)entity);
        }
    }
}

public class EntityFactory<TComponent> : EntityFactory<TComponent, IDataLayer<TComponent>>
{
}

public class EntityFactory : EntityFactory<object>
{
}
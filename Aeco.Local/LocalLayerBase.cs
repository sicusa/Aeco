namespace Aeco.Local;

public abstract class LocalLayerBase<TComponent> : ILayer<TComponent>
{
    private Dictionary<Guid, IEntity<TComponent>>? _entities;

    public IReadOnlyEntity<TComponent> GetReadOnlyEntity(Guid id)
        => GetEntity(id);
    
    public IEntity<TComponent> GetEntity(Guid id)
    {
        if (_entities == null) {
            _entities = new Dictionary<Guid, IEntity<TComponent>>();
        }
        if (_entities.TryGetValue(id, out var entity)) {
            return entity;
        }
        entity = RawCreateEntity(id);
        entity.Disposed.Subscribe(OnEntityDisposed);
        return entity;
    }

    private void OnEntityDisposed(IEntity<TComponent> entity)
    {
        _entities!.Remove(entity.Id);
    }

    protected abstract IEntity<TComponent> RawCreateEntity(Guid id);
}
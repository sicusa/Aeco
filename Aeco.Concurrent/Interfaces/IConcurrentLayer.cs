namespace Aeco.Concurrent;

public interface IConcurrentLayer<in TComponent> : ILayer<TComponent>
{
    ReaderWriterLockSlim LockSlim { get; }
    IConcurrentEntity<TComponent> GetConcurrentEntity(Guid id);
}

public interface IConcurrentDataLayer<in TComponent>
    : IDataLayer<TComponent>, IConcurrentLayer<TComponent>
{
    IConcurrentEntity<TComponent> GetConcurrentEntity<UComponent>()
        where UComponent : TComponent;
}

public static class ConcurrentLayerExtensions
{
    public static IEntity<TComponent> CreateConcurrentEntity<TComponent>(this IConcurrentLayer<TComponent> layer)
        => layer.GetConcurrentEntity(Guid.NewGuid());
    public static IEntity<TComponent> GetConcurrentEntity<TComponent>(this IConcurrentLayer<TComponent> layer, string id)
        => layer.GetConcurrentEntity(Guid.Parse(id));
}
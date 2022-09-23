namespace Aeco.Concurrent;

public interface IReadOnlyConcurrentLayer<in TComponent> : IReadOnlyLayer<TComponent>
{
    IReadOnlyConcurrentEntity<TComponent> GetReadOnlyConcurrentEntity(Guid id);
}

public interface IConcurrentLayer<in TComponent>
    : IReadOnlyConcurrentLayer<TComponent>, ILayer<TComponent>
{
    ReaderWriterLockSlim LockSlim { get; }
    IConcurrentEntity<TComponent> GetConcurrentEntity(Guid id);
}

public interface IReadOnlyConcurrentDataLayer<in TComponent>
    : IReadOnlyDataLayer<TComponent>, IReadOnlyConcurrentLayer<TComponent>
{
    IReadOnlyConcurrentEntity<TComponent> GetReadOnlyConcurrentEntity<UComponent>()
        where UComponent : TComponent;
}

public interface IConcurrentDataLayer<in TComponent>
    : IReadOnlyConcurrentDataLayer<TComponent>, IDataLayer<TComponent>, IConcurrentLayer<TComponent>
{
    IConcurrentEntity<TComponent> GetConcurrentEntity<UComponent>()
        where UComponent : TComponent;
}

public static class ConcurrentLayerExtensions
{
    public static IEntity<TComponent> CreateConcurrentEntity<TComponent>(this IConcurrentLayer<TComponent> layer)
        => layer.GetConcurrentEntity(Guid.NewGuid());
    public static IConcurrentEntity<TComponent> GetReadOnlyConcurrentEntity<TComponent>(this IConcurrentLayer<TComponent> layer, string id)
        => layer.GetConcurrentEntity(Guid.Parse(id));
    public static IEntity<TComponent> GetConcurrentEntity<TComponent>(this IConcurrentLayer<TComponent> layer, string id)
        => layer.GetConcurrentEntity(Guid.Parse(id));
}
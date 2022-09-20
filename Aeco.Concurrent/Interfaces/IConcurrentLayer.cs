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
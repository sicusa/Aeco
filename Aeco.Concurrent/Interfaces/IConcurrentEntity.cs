namespace Aeco.Concurrent;

public interface IReadOnlyConcurrentEntity<in TComponent> : IReadOnlyEntity<TComponent>
{
    void Require<UComponent>(Action<UComponent> callback)
        where UComponent : TComponent;
}

public interface IConcurrentEntity<in TComponent>
    : IReadOnlyConcurrentEntity<TComponent>, IEntity<TComponent>
{
}
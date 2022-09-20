namespace Aeco.Concurrent;

using System.Diagnostics.CodeAnalysis;

public interface IConcurrentEntity<in TComponent> : IEntity<TComponent>
{
    void Require<UComponent>(Action<UComponent> callback)
        where UComponent : TComponent;
    void Acquire<UComponent>(Action<UComponent> callback)
        where UComponent : TComponent, new();
}

public interface IConcurrentMortalEntity<in TComponent> : IEntity<TComponent>
{
    bool IsDestroyed { get; }
    bool Destroy();
}
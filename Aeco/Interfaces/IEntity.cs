namespace Aeco;

using System;
using System.Diagnostics.CodeAnalysis;

public interface IEntity<in TComponent> : IDisposable
{
    Guid Id { get; }
    IEnumerable<object> Components { get; }
    IObservable<IEntity<TComponent>> Disposed { get; }

    bool TryGet<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    ref UComponent Require<UComponent>()
        where UComponent : TComponent;
    ref UComponent Acquire<UComponent>()
        where UComponent : TComponent, new();
    bool Contains<UComponent>()
        where UComponent : TComponent;
    bool Remove<UComponent>()
        where UComponent : TComponent;
    void Set<UComponent>(in UComponent component)
        where UComponent : TComponent;

    void Clear();
}

public interface IMortalEntity<in TComponent> : IEntity<TComponent>
{
    bool IsDestroyed { get; }
    bool Destroy();
}
namespace Aeco;

using System;
using System.Diagnostics.CodeAnalysis;

public interface IReadOnlyEntity<in TComponent> : IDisposable
{
    Guid Id { get; }
    IObservable<IEntity<TComponent>> Disposed { get; }

    bool TryGet<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    ref readonly UComponent Inspect<UComponent>()
        where UComponent : TComponent;
    bool Contains<UComponent>()
        where UComponent : TComponent;
}

public interface IEntity<in TComponent> : IReadOnlyEntity<TComponent>
{
    IEnumerable<object> Components { get; }

    ref UComponent Require<UComponent>()
        where UComponent : TComponent;
    ref UComponent Acquire<UComponent>()
        where UComponent : TComponent, new();
    ref UComponent Acquire<UComponent>(out bool exists)
        where UComponent : TComponent, new();
    bool Remove<UComponent>()
        where UComponent : TComponent;
    bool Remove<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    void Set<UComponent>(in UComponent component)
        where UComponent : TComponent;

    void Clear();
}
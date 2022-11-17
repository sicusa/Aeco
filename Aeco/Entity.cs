namespace Aeco;

using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

public class Entity<TComponent, TDataLayer> : IEntity<TComponent>
    where TDataLayer : IDataLayer<TComponent>
{
    public Guid Id { get; private set; }
    public IEnumerable<object> Components => _dataLayer.GetAll(Id);

    public IDataLayer<TComponent> DataLayer => _dataLayer;
    public IObservable<IEntity<TComponent>> Disposed => _disposed;

    private TDataLayer _dataLayer;
    private Subject<IEntity<TComponent>> _disposed = new();

    public Entity(TDataLayer dataLayer, Guid id)
    {
        _dataLayer = dataLayer;
        Id = id;
    }

    public void Reset(TDataLayer dataLayer, Guid id)
    {
        Id = id;
        _dataLayer = dataLayer;
        _disposed = new();
    }

    public void Dispose()
    {
        if (!_disposed.IsDisposed) {
            _disposed.OnNext(this);
            _disposed.Dispose();
        }
    }

    public void Clear()
        => _dataLayer.Clear(Id);

    public bool TryGet<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
        => _dataLayer.TryGet<UComponent>(Id, out component);

    public ref readonly UComponent Inspect<UComponent>()
        where UComponent : TComponent
        => ref _dataLayer.Inspect<UComponent>(Id);

    public ref UComponent UnsafeInspect<UComponent>()
        where UComponent : TComponent
        => ref _dataLayer.UnsafeInspect<UComponent>(Id);

    public ref UComponent Require<UComponent>()
        where UComponent : TComponent
        => ref _dataLayer.Require<UComponent>(Id);

    public ref UComponent Acquire<UComponent>()
        where UComponent : TComponent, new()
        => ref _dataLayer.Acquire<UComponent>(Id);

    public ref UComponent Acquire<UComponent>(out bool exists)
        where UComponent : TComponent, new()
        => ref _dataLayer.Acquire<UComponent>(Id, out exists);

    public ref UComponent UnsafeAcquire<UComponent>()
        where UComponent : TComponent, new()
        => ref _dataLayer.UnsafeAcquire<UComponent>(Id);

    public ref UComponent UnsafeAcquire<UComponent>(out bool exists)
        where UComponent : TComponent, new()
        => ref _dataLayer.UnsafeAcquire<UComponent>(Id, out exists);
    
    public bool Contains<UComponent>()
        where UComponent : TComponent
        => _dataLayer.Contains<UComponent>(Id);

    public bool Remove<UComponent>()
        where UComponent : TComponent
        => _dataLayer.Remove<UComponent>(Id);

    public bool Remove<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
        => _dataLayer.Remove<UComponent>(Id, out component);
    
    public void Set<UComponent>(in UComponent component)
        where UComponent : TComponent
        => _dataLayer.Set(Id, component);
}

public class Entity<TComponent>
    : Entity<TComponent, IDataLayer<TComponent>>
{
    public Entity(IDataLayer<TComponent> dataLayer, Guid id)
        : base(dataLayer, id)
    {
    }
}

public class Entity : Entity<IComponent>
{
    public Entity(IDataLayer<IComponent> dataLayer, Guid id)
        : base(dataLayer, id)
    {
    }
}
namespace Aeco.Concurrent;

using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

public class ConcurrentEntity<TComponent, TDataLayer> : IConcurrentEntity<TComponent>
    where TDataLayer : IConcurrentDataLayer<TComponent>
{
    public Guid Id { get; private set; }
    public IEnumerable<object> Components {
        get {
            var lockSlim = _dataLayer.LockSlim;
            lockSlim.EnterReadLock();
            try {
                return _dataLayer.GetAll(Id);
            }
            finally {
                lockSlim.ExitReadLock();
            }
        }
    }

    public IDataLayer<TComponent> DataLayer => _dataLayer;
    public IObservable<IEntity<TComponent>> Disposed => _disposed;

    private TDataLayer _dataLayer;
    private Subject<IEntity<TComponent>> _disposed = new();

    public ConcurrentEntity(TDataLayer dataLayer, Guid id)
    {
        _dataLayer = dataLayer;
        Id = id;
    }

    public void Reset(TDataLayer dataLayer, Guid id)
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();

        Id = id;
        _dataLayer = dataLayer;
        _disposed = new();

        lockSlim.ExitWriteLock();
    }

    public void Dispose()
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();
        try {
            if (!_disposed.IsDisposed) {
                _disposed.OnNext(this);
                _disposed.Dispose();
            }
        }
        finally {
            lockSlim.ExitWriteLock();
        }
    }

    public void Clear()
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();
        try {
            _dataLayer.Clear(Id);
        }
        finally {
            lockSlim.ExitWriteLock();
        }
    }

    public bool TryGet<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterReadLock();
        try {
            return _dataLayer.TryGet<UComponent>(Id, out component);
        }
        finally {
            lockSlim.ExitReadLock();
        }
    }

    public void Require<UComponent>(Action<UComponent> callback)
        where UComponent : TComponent
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterReadLock();
        try {
            ref var comp = ref _dataLayer.Require<UComponent>(Id);
            callback(comp);
        }
        finally {
            lockSlim.ExitReadLock();
        }
    }

    public ref UComponent Require<UComponent>()
        where UComponent : TComponent
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterReadLock();
        try {
            return ref _dataLayer.Require<UComponent>(Id);
        }
        finally {
            lockSlim.ExitReadLock();
        }
    }

    public void Acquire<UComponent>(Action<UComponent> callback)
        where UComponent : TComponent, new()
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();
        try {
            ref var comp = ref _dataLayer.Acquire<UComponent>(Id);
            callback(comp);
        }
        finally {
            lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent Acquire<UComponent>()
        where UComponent : TComponent, new()
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();
        try {
            return ref _dataLayer.Acquire<UComponent>(Id);
        }
        finally {
            lockSlim.ExitWriteLock();
        }
    }
    
    public bool Contains<UComponent>()
        where UComponent : TComponent
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterReadLock();
        try {
            return _dataLayer.Contains<UComponent>(Id);
        }
        finally {
            lockSlim.ExitReadLock();
        }
    }

    public bool Remove<UComponent>()
        where UComponent : TComponent
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();
        try {
            return _dataLayer.Remove<UComponent>(Id);
        }
        finally {
            lockSlim.ExitWriteLock();
        }
    }
    
    public void Set(TComponent component)
    {
        var lockSlim = _dataLayer.LockSlim;
        lockSlim.EnterWriteLock();
        try {
            _dataLayer.Set(Id, component);
        }
        finally {
            lockSlim.ExitWriteLock();
        }
    }
}

public class ConcurrentEntity<TComponent>
    : ConcurrentEntity<TComponent, IConcurrentDataLayer<TComponent>>
{
    public ConcurrentEntity(IConcurrentDataLayer<TComponent> dataLayer, Guid id)
        : base(dataLayer, id)
    {
    }
}

public class ConcurrentEntity : ConcurrentEntity<object>
{
    public ConcurrentEntity(IConcurrentDataLayer<object> dataLayer, Guid id)
        : base(dataLayer, id)
    {
    }
}
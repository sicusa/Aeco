namespace Aeco.Concurrent;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public class ConcurrentDataLayer<TComponent> : IDataLayer<TComponent>
{
    private ReaderWriterLockSlim _lockSlim = new();
    private IDataLayer<TComponent> _inner;

    public ConcurrentDataLayer(IDataLayer<TComponent> inner)
    {
        _inner = inner;
    }

    public bool CheckSupported(Type componentType)
        =>  _inner.CheckSupported(componentType);
    public IEntity<TComponent> GetEntity<UComponent>() where UComponent : TComponent
        => _inner.GetEntity<UComponent>();
    public IReadOnlyEntity<TComponent> GetReadOnlyEntity<UComponent>() where UComponent : TComponent
        => _inner.GetReadOnlyEntity<UComponent>();

    public IEnumerable<Guid> Query() => _inner.Query();
    public IEntity<TComponent> GetEntity(Guid id) => _inner.GetEntity(id);
    public IReadOnlyEntity<TComponent> GetReadOnlyEntity(Guid id) => _inner.GetReadOnlyEntity(id);

    public bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.TryGet(entityId, out component);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref readonly UComponent Inspect<UComponent>(Guid entityId)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.Inspect<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.InspectAny<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref UComponent UnsafeInspect<UComponent>(Guid entityId)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.UnsafeInspect<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref UComponent UnsafeInspectAny<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.UnsafeInspectAny<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public bool Contains<UComponent>(Guid entityId)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.Contains<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public bool ContainsAny<UComponent>() where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.ContainsAny<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref UComponent Require<UComponent>(Guid entityId)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.Require<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.RequireAny<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref UComponent Acquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.Acquire<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.Acquire<UComponent>(entityId, out exists);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent AcquireAny<UComponent>()
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.AcquireAny<UComponent>();
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent AcquireAny<UComponent>(out bool exists)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.AcquireAny<UComponent>(out exists);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent UnsafeAcquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.UnsafeAcquire<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent UnsafeAcquire<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.UnsafeAcquire<UComponent>(entityId, out exists);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public bool Remove<UComponent>(Guid entityId)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return _inner.Remove<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return _inner.Remove(entityId, out component);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public bool RemoveAny<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return _inner.RemoveAny<UComponent>();
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public bool RemoveAny<UComponent>([MaybeNullWhen(false)] out UComponent component) where UComponent : TComponent
    {
        return _inner.RemoveAny(out component);
    }

    public void RemoveAll<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            _inner.RemoveAll<UComponent>();
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.Set(entityId, component);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.SetAny(component);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public void Clear(Guid entityId)
    {
        _lockSlim.EnterWriteLock();
        try {
            _inner.Clear(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lockSlim.EnterWriteLock();
        try {
            _inner.Clear();
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public IEnumerable<object> GetAll(Guid entityId)
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.GetAll(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public Guid? Singleton<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.Singleton<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public int GetCount()
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.GetCount();
        }
        finally { 
            _lockSlim.ExitReadLock();
        }
    }

    public int GetCount<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.GetCount<UComponent>();
        }
        finally { 
            _lockSlim.ExitReadLock();
        }
    }

    public IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.Query<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }
}

public class ConcurrentDataLayer : ConcurrentDataLayer<IComponent>
{
    public ConcurrentDataLayer(IDataLayer<IComponent> inner)
        : base(inner)
    {
    }
}
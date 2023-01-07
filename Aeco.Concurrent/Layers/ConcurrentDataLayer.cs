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

    public bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.TryGet(id, out component);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref readonly UComponent Inspect<UComponent>(Guid id)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.Inspect<UComponent>(id);
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

    public ref UComponent InspectRaw<UComponent>(Guid id)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.InspectRaw<UComponent>(id);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.InspectAnyRaw<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public bool Contains<UComponent>(Guid id)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.Contains<UComponent>(id);
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

    public ref UComponent Require<UComponent>(Guid id)
        where UComponent : TComponent
    {
        _lockSlim.EnterReadLock();
        try {
            return ref _inner.Require<UComponent>(id);
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

    public ref UComponent Acquire<UComponent>(Guid id)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.Acquire<UComponent>(id);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.Acquire<UComponent>(id, out exists);
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

    public ref UComponent AcquireRaw<UComponent>(Guid id)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.AcquireRaw<UComponent>(id);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.AcquireRaw<UComponent>(id, out exists);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public bool Remove<UComponent>(Guid id)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return _inner.Remove<UComponent>(id);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return _inner.Remove(id, out component);
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

    public ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref _inner.Set(id, component);
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

    public void Clear(Guid id)
    {
        _lockSlim.EnterWriteLock();
        try {
            _inner.Clear(id);
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

    public IEnumerable<object> GetAll(Guid id)
    {
        _lockSlim.EnterReadLock();
        try {
            return _inner.GetAll(id);
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
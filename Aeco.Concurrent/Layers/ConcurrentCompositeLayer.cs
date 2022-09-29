namespace Aeco.Concurrent;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aeco.Local;

public class ConcurrentCompositeLayer<TComponent, TSublayer> : CompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    private ReaderWriterLockSlim _lockSlim = new();

    public ConcurrentCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        _lockSlim.EnterReadLock();
        try {
            return base.TryGet(entityId, out component);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public override ref readonly UComponent Inspect<UComponent>(Guid entityId)
    {
        _lockSlim.EnterReadLock();
        try {
            return ref base.Inspect<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public override bool Contains<UComponent>(Guid entityId)
    {
        _lockSlim.EnterReadLock();
        try {
            return base.Contains<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        _lockSlim.EnterReadLock();
        try {
            return ref base.Require<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref base.Acquire<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref base.Acquire<UComponent>(entityId, out exists);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        _lockSlim.EnterWriteLock();
        try {
            return base.Remove<UComponent>(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        _lockSlim.EnterWriteLock();
        try {
            return base.Remove(entityId, out component);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
    {
        _lockSlim.EnterWriteLock();
        try {
            return ref base.Set(entityId, component);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override void Clear(Guid entityId)
    {
        _lockSlim.EnterWriteLock();
        try {
            base.Clear(entityId);
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override void Clear()
    {
        _lockSlim.EnterWriteLock();
        try {
            base.Clear();
        }
        finally {
            _lockSlim.ExitWriteLock();
        }
    }

    public override IEnumerable<object> GetAll(Guid entityId)
    {
        _lockSlim.EnterReadLock();
        try {
            return base.GetAll(entityId);
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public override Guid Singleton<UComponent>()
    {
        _lockSlim.EnterReadLock();
        try {
            return base.Singleton<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }

    public override IEnumerable<Guid> Query<UComponent>()
    {
        _lockSlim.EnterReadLock();
        try {
            return base.Query<UComponent>();
        }
        finally {
            _lockSlim.ExitReadLock();
        }
    }
}

public class ConcurrentCompositeLayer<TComponent> : ConcurrentCompositeLayer<TComponent, ILayer<TComponent>>
{
    public ConcurrentCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class ConcurrentCompositeLayer : ConcurrentCompositeLayer<IComponent>
{
    public ConcurrentCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
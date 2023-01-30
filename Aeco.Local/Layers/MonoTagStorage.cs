namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class MonoTagStorage<TComponent, TStoredComponent>
    : MonoStorageBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    private SortedSet<Guid> _ids = new();
    private TStoredComponent _instance = new();

    private Guid? _singleton;

    public override bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (_ids.Contains(id)) {
            component = _instance;
            return true;
        }
        component = default;
        return false;
    }

    public override ref TStoredComponent RequireOrNullRef(Guid id)
    {
        if (_ids.Contains(id)) {
            return ref _instance;
        }
        return ref Unsafe.NullRef<TStoredComponent>();
    }

    public override ref TStoredComponent Acquire(Guid id)
        => ref Acquire(id, out bool _);
    
    public override ref TStoredComponent Acquire(Guid id, out bool exists)
    {
        exists = !_ids.Add(id);
        return ref _instance;
    }

    public override bool Contains(Guid id)
        => _ids.Contains(id);

    public override bool ContainsAny()
        => _ids.Count != 0;

    private bool RawRemove(Guid id)
    {
        if (!_ids.Remove(id)) {
            return false;
        }
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override bool Remove(Guid id)
        => RawRemove(id);

    public override bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (!_ids.Remove(id)) {
            component = default;
            return false;
        }
        if (_singleton == id) {
            _singleton = null;
        }
        component = _instance;
        return true;
    }

    public override ref TStoredComponent Set(Guid id, in TStoredComponent component)
    {
        _ids.Add(id);
        _instance = component;
        return ref _instance;
    }

    private Guid? ResetSingleton()
    {
        if (_ids.Count != 0) {
            _singleton = _ids.First();
        }
        return _singleton;
    }

    public override Guid? Singleton()
        => _singleton == null ? ResetSingleton() : _singleton;

    public override IEnumerable<Guid> Query()
        => _ids;

    public override int GetCount()
        => _ids.Count;

    public override IEnumerable<object> GetAll(Guid id)
    {
        if (!_ids.Contains(id)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(_instance!, 1);
    }

    public override void Clear(Guid id)
        => RawRemove(id);

    public override void Clear()
    {
        _ids.Clear();
        _singleton = null;
    }
}

public class MonoTagStorage<TStoredComponent> : MonoTagStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, new()
{
}

public static class MonoTagStorage
{
    public class Factory<TComponent> : IDataLayerFactory<TComponent>
    {
        public static Factory<TComponent> Shared { get; } = new();

        public IBasicDataLayer<TComponent> Create<TStoredComponent>()
            where TStoredComponent : TComponent, new()
            => new MonoTagStorage<TComponent, TStoredComponent>();
    }
}
namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class MonoHashStorage<TComponent, TStoredComponent>
    : MonoStorageBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    private SortedSet<uint> _ids = new();
    private Dictionary<uint, TStoredComponent> _dict = new();

    private uint? _singleton;

    public override bool TryGet(uint id, [MaybeNullWhen(false)] out TStoredComponent component)
        => _dict.TryGetValue(id, out component);

    public override ref TStoredComponent RequireOrNullRef(uint id)
        => ref CollectionsMarshal.GetValueRefOrNullRef(_dict, id);

    public override ref TStoredComponent Acquire(uint id)
        => ref Acquire(id, out bool _);
    
    public override ref TStoredComponent Acquire(uint id, out bool exists)
    {
        ref var comp = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, id, out exists);
        if (!exists) {
            comp = new();
            _ids.Add(id);
        }
        return ref comp!;
    }

    public override bool Contains(uint id)
        => _ids.Contains(id);

    public override bool ContainsAny()
        => _ids.Count != 0;

    private bool RawRemove(uint id)
    {
        if (!_ids.Remove(id)) {
            return false;
        }
        _dict.Remove(id);
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override bool Remove(uint id)
        => RawRemove(id);

    public override bool Remove(uint id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (!_ids.Remove(id)) {
            component = default;
            return false;
        }
        if (!_dict.Remove(id, out component)) {
            throw new InvalidOperationException("Internal error");
        }
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override ref TStoredComponent Set(uint id, in TStoredComponent component)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, id, out bool exists);
        if (!exists) {
            _ids.Add(id);
        }
        value = component;
        return ref value!;
    }

    private uint? ResetSingleton()
    {
        if (_ids.Count != 0) {
            _singleton = _ids.First();
        }
        return _singleton;
    }

    public override uint? Singleton()
        => _singleton == null ? ResetSingleton() : _singleton;

    public override IEnumerable<uint> Query()
        => _ids;

    public override int GetCount()
        => _ids.Count;

    public override IEnumerable<object> GetAll(uint id)
    {
        ref var comp = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, id);
        if (Unsafe.IsNullRef(ref comp)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(comp!, 1);
    }

    public override void Clear(uint id)
        => RawRemove(id);

    public override void Clear()
    {
        _dict.Clear();
        _ids.Clear();
        _singleton = null;
    }
}

public class MonoHashStorage<TStoredComponent> : MonoHashStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, new()
{
}

public static class MonoHashStorage
{
    public class Factory<TComponent> : IDataLayerFactory<TComponent>
    {
        public static Factory<TComponent> Shared { get; } = new();

        public IBasicDataLayer<TComponent> Create<TStoredComponent>()
            where TStoredComponent : TComponent, new()
            => new MonoHashStorage<TComponent, TStoredComponent>();
    }
}
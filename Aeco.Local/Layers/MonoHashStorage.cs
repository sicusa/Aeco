namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class MonoHashStorage<TComponent, TStoredComponent>
    : LocalMonoDataLayerBase<TComponent, TStoredComponent>, IStableDataLayer<TComponent>
    where TStoredComponent : TComponent, new()
{
    private SortedSet<Guid> _ids = new();
    private Dictionary<Guid, TStoredComponent> _dict = new();

    private Guid? _singleton;

    public override bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
        => _dict.TryGetValue(id, out component);

    public override ref TStoredComponent Require(Guid id)
    {
        ref var comp = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, id);
        if (Unsafe.IsNullRef(ref comp)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref comp!;
    }

    public override ref TStoredComponent Acquire(Guid id)
        => ref Acquire(id, out bool _);
    
    public override ref TStoredComponent Acquire(Guid id, out bool exists)
    {
        ref var comp = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, id, out exists);
        if (!exists) {
            comp = new();
            _ids.Add(id);
        }
        return ref comp!;
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
        _dict.Remove(id);
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
        if (!_dict.Remove(id, out component)) {
            throw new InvalidOperationException("Internal error");
        }
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override ref TStoredComponent Set(Guid id, in TStoredComponent component)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, id, out bool exists);
        value = component;
        return ref value!;
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
        ref var comp = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, id);
        if (Unsafe.IsNullRef(ref comp)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(comp!, 1);
    }

    public override void Clear(Guid id)
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

        public IDataLayer<TComponent> Create<TStoredComponent>()
            where TStoredComponent : TComponent, new()
            => new MonoHashStorage<TComponent, TStoredComponent>();
    }
}
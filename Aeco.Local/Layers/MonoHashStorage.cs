namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class MonoHashStorage<TComponent, TStoredComponent> : LocalMonoDataLayerBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    private Dictionary<Guid, TStoredComponent> _dict = new();
    private SortedSet<Guid> _entityIds = new();

    private Guid? _singleton;
    private bool _existsTemp;

    public override bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
        => _dict.TryGetValue(id, out component);

    public override ref TStoredComponent Require(Guid id)
    {
        ref TStoredComponent comp = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, id);
        if (Unsafe.IsNullRef(ref comp)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref comp;
    }

    public override ref TStoredComponent Acquire(Guid id)
        => ref Acquire(id, out _existsTemp);
    
    public override ref TStoredComponent Acquire(Guid id, out bool exists)
    {
        ref TStoredComponent? comp = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, id, out exists);
        if (!exists) {
            comp = new TStoredComponent();
            _entityIds.Add(id);
        }
        return ref comp!;
    }

    public override bool Contains(Guid id)
        => _entityIds.Contains(id);

    public override bool ContainsAny()
        => _singleton != null;

    private bool RawRemove(Guid id)
    {
        if (!_entityIds.Remove(id)) {
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
        if (!_entityIds.Remove(id)) {
            component = default;
            return false;
        }
        if (!_dict.Remove(id, out component)) {
            throw new KeyNotFoundException("Internal error");
        }
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override ref TStoredComponent Set(Guid id, in TStoredComponent component)
    {
        ref TStoredComponent? value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, id, out _existsTemp);
        value = component;
        return ref value!;
    }

    private Guid? ResetSingleton()
    {
        if (_entityIds.Count != 0) {
            _singleton = _entityIds.First();
        }
        return _singleton;
    }

    public override Guid? Singleton()
        => _singleton == null ? ResetSingleton() : _singleton;

    public override IEnumerable<Guid> Query()
        => _entityIds;

    public override int GetCount()
        => _entityIds.Count;

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
        _entityIds.Clear();
        _singleton = null;
    }
}

public class MonoHashStorage<TStoredComponent> : MonoHashStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, new()
{
}

public static class MonoHashStorage
{
    public static IDataLayer<TComponent> CreateUnsafe<TComponent>(Type selectedComponentType)
    {
        var type = typeof(MonoHashStorage<,>).MakeGenericType(
            new Type[] {typeof(TComponent), selectedComponentType});
        return (IDataLayer<TComponent>)Activator.CreateInstance(type)!;
    }

    public static Func<Type, IDataLayer<TComponent>> MakeUnsafeCreator<TComponent>()
        => selectedComponentType =>
            CreateUnsafe<TComponent>(selectedComponentType);
}
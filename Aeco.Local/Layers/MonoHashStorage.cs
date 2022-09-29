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

    private Guid _singleton;
    private bool _existsTemp;

    public override bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
        => _dict.TryGetValue(entityId, out component);

    public override ref TStoredComponent Require(Guid entityId)
    {
        ref TStoredComponent comp = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, entityId);
        if (Unsafe.IsNullRef(ref comp)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref comp;
    }

    public override ref TStoredComponent Acquire(Guid entityId)
        => ref Acquire(entityId, out _existsTemp);
    
    public override ref TStoredComponent Acquire(Guid entityId, out bool exists)
    {
        ref TStoredComponent? comp = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, entityId, out exists);
        if (!exists) {
            comp = new TStoredComponent();
            _entityIds.Add(entityId);
            if (_singleton == Guid.Empty) {
                _singleton = entityId;
            }
        }
        return ref comp!;
    }

    public override bool Contains(Guid entityId)
        => _entityIds.Contains(entityId);

    public override bool Contains()
        => _singleton != Guid.Empty;

    private void ResetSingleton()
    {
        _singleton = _entityIds.Count != 0 ? _entityIds.First() : Guid.Empty;
    }

    private bool RawRemove(Guid entityId)
    {
        if (!_entityIds.Remove(entityId)) {
            return false;
        }
        _dict.Remove(entityId);
        if (_singleton == entityId) {
            ResetSingleton();
        }
        return true;
    }

    public override bool Remove(Guid entityId)
        => RawRemove(entityId);

    public override bool Remove(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (!_entityIds.Remove(entityId)) {
            component = default;
            return false;
        }
        if (!_dict.Remove(entityId, out component)) {
            throw new KeyNotFoundException("Internal error");
        }

        if (_singleton == entityId) {
            ResetSingleton();
        }
        return true;
    }

    public override ref TStoredComponent Set(Guid entityId, in TStoredComponent component)
    {
        ref TStoredComponent? value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, entityId, out _existsTemp);
        value = component;

        if (_singleton == Guid.Empty) {
            _singleton = entityId;
        }

        return ref value!;
    }

    public override Guid Singleton()
        => _singleton != Guid.Empty ? _singleton
            : throw new KeyNotFoundException("Singleton not found");

    public override IEnumerable<Guid> Query()
        => _entityIds;

    public override IEnumerable<object> GetAll(Guid entityId)
    {
        ref var comp = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, entityId);
        if (Unsafe.IsNullRef(ref comp)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(comp!, 1);
    }

    public override void Clear(Guid entityId)
        => RawRemove(entityId);

    public override void Clear()
    {
        _dict.Clear();
        _entityIds.Clear();
        _singleton = Guid.Empty;
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
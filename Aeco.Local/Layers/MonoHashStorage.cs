namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class MonoHashStorage<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    private Dictionary<Guid, TSelectedComponent> _dict = new();
    private SortedSet<Guid> _entityIds = new();

    private Guid _singleton;
    private bool _existsTemp;

    public override bool CheckSupported(Type componentType)
        => typeof(TSelectedComponent) == componentType;

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var convertedDict = _dict as Dictionary<Guid, UComponent>
            ?? throw new NotSupportedException("Component not supported");
        return convertedDict.TryGetValue(entityId, out component);
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        var convertedDict = _dict as Dictionary<Guid, UComponent>
            ?? throw new NotSupportedException("Component not supported");

        ref UComponent comp = ref CollectionsMarshal.GetValueRefOrNullRef(convertedDict, entityId);
        if (Unsafe.IsNullRef(ref comp)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
        => ref Acquire<UComponent>(entityId, out _existsTemp);
    
    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        var convertedDict = _dict as Dictionary<Guid, UComponent>
            ?? throw new NotSupportedException("Component not supported");

        ref UComponent comp = ref CollectionsMarshal.GetValueRefOrNullRef(convertedDict, entityId);
        if (Unsafe.IsNullRef(ref comp)) {
            convertedDict.Add(entityId, new UComponent());
            _entityIds.Add(entityId);
            if (_singleton == Guid.Empty) {
                _singleton = entityId;
            }
            exists = false;
            return ref CollectionsMarshal.GetValueRefOrNullRef(convertedDict, entityId);
        }
        exists = true;
        return ref comp;
    }

    public override bool Contains<UComponent>(Guid entityId)
        => _entityIds.Contains(entityId);

    public override bool Contains<UComponent>()
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

    public override bool Remove<UComponent>(Guid entityId)
        => RawRemove(entityId);

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (!_entityIds.Remove(entityId)) {
            component = default;
            return false;
        }

        var convertedDict = _dict as Dictionary<Guid, UComponent>
            ?? throw new NotSupportedException("Component not supported");
        if (!convertedDict.Remove(entityId, out component)) {
            throw new KeyNotFoundException("Internal error");
        }

        if (_singleton == entityId) {
            ResetSingleton();
        }
        return true;
    }

    public override void Set<UComponent>(Guid entityId, in UComponent component)
    {
        var convertedDict = _dict as Dictionary<Guid, UComponent>
            ?? throw new NotSupportedException("Component not supported");
        
        if (convertedDict.ContainsKey(entityId)) {
            convertedDict[entityId] = component;
            return;
        }
        convertedDict.Add(entityId, component);
        if (_singleton == Guid.Empty) {
            _singleton = entityId;
        }
    }

    public override Guid Singleton<UComponent>()
        => _singleton != Guid.Empty ? _singleton
            : throw new KeyNotFoundException("Singleton not found");

    public override IEnumerable<Guid> Query<UComponent>()
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

public class MonoHashStorage<TSelectedComponent> : MonoHashStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
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
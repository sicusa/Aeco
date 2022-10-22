namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public class MonoPoolStorage<TComponent, TStoredComponent> : LocalMonoDataLayerBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    public int BrickCapacity { get; set; }

    private FastHashBrick<Guid, TStoredComponent> _brick;
    private SortedSet<Guid> _entityIds = new();

    private int _cellarCount;
    private Guid _singleton = Guid.Empty;
    private bool _existsTemp;

    public MonoPoolStorage(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
    {
        BrickCapacity = brickCapacity;
        _cellarCount = FastHashBrick.CalculateProperCellarCount(brickCapacity);
        _brick = new(brickCapacity);
    }
    
    private int GetIndex(Guid entityId)
        => FastHashBrick.CalculateIndex(entityId.GetHashCode(), _cellarCount);

    public override bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.FindBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            component = default;
            return false;
        }
        component = block.Value;
        return true;
    }

    public override ref TStoredComponent Require(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.FindBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref block.Value;
    }
    
    public override ref TStoredComponent Acquire(Guid entityId)
        => ref Acquire(entityId, out _existsTemp);

    public override ref TStoredComponent Acquire(Guid entityId, out bool exists)
    {
        ref var block = ref _brick.AcquireBlock(GetIndex(entityId), entityId, out exists);
        if (!exists) {
            block.Value = new();
            _entityIds.Add(entityId);
            if (_singleton == Guid.Empty) {
                _singleton = entityId;
            }
        }
        return ref block.Value;
    }

    public override bool Contains(Guid entityId)
        => _entityIds.Contains(entityId);

    public override bool ContainsAny()
        => _singleton != Guid.Empty;

    private bool ResetSingleton()
    {
        if (_entityIds.Count != 0) {
            _singleton = _entityIds.First();
            return true;
        }
        return false;
    }

    private void ClearBlock(ref FastHashBrick<Guid, TStoredComponent>.Block block, in Guid entityId)
    {
        block.Value = default!;
        _entityIds.Remove(entityId);
        if (_singleton == entityId) {
            _singleton = Guid.Empty;
        }
    }

    public override bool Remove(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.RemoveBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            return false;
        }
        ClearBlock(ref block, entityId);
        return true;
    }

    public override bool Remove(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.RemoveBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            component = default;
            return false;
        }
        component = block.Value;
        ClearBlock(ref block, entityId);
        return true;
    }

    public override ref TStoredComponent Set(Guid entityId, in TStoredComponent component)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.AcquireBlock(index, entityId, out _existsTemp);
        if (!_existsTemp) {
            _entityIds.Add(entityId);
            ResetSingleton();
        }
        block.Value = component;
        return ref block.Value;
    }

    public override Guid Singleton()
    {
        if (_singleton == Guid.Empty && !ResetSingleton()) {
            throw new KeyNotFoundException("Singleton not found");
        }
        return _singleton;
    }

    public override IEnumerable<Guid> Query()
        => _entityIds;

    public override IEnumerable<object> GetAll(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.FindBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(block.Value!, 1);
    }

    public override void Clear(Guid entityId)
        => Remove(entityId);

    public override void Clear()
    {
        _brick.Clear();
        _entityIds.Clear();
        _singleton = Guid.Empty;
    }
}

public class MonoPoolStorage<TStoredComponent> : MonoPoolStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, IDisposable, new()
{
}

public static class MonoPoolStorage
{
    public const int DefaultBrickCapacity = 521;

    public static IDataLayer<TComponent> CreateUnsafe<TComponent>(Type selectedComponentType, int capacity = DefaultBrickCapacity)
    {
        var type = typeof(MonoPoolStorage<,>).MakeGenericType(
            new Type[] {typeof(TComponent), selectedComponentType});
        return (IDataLayer<TComponent>)Activator.CreateInstance(type, new object[] {capacity})!;
    }

    public static Func<Type, IDataLayer<TComponent>> MakeUnsafeCreator<TComponent>(int capacity = DefaultBrickCapacity)
        => selectedComponentType =>
            CreateUnsafe<TComponent>(selectedComponentType, capacity);
}
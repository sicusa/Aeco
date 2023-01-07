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

    private Guid? _singleton;
    private bool _existsTemp;

    public MonoPoolStorage(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
    {
        BrickCapacity = brickCapacity;
        _brick = new(brickCapacity);
    }

    public override bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
        => _brick.TryGetValue(id, out component);

    public override ref TStoredComponent Require(Guid id)
    {
        ref var block = ref _brick.FindBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref block.Value;
    }
    
    public override ref TStoredComponent Acquire(Guid id)
        => ref Acquire(id, out _existsTemp);

    public override ref TStoredComponent Acquire(Guid id, out bool exists)
    {
        ref var block = ref _brick.AcquireBlock(id, out exists);
        if (!exists) {
            block.Value = new();
            _entityIds.Add(id);
        }
        return ref block.Value;
    }

    public override bool Contains(Guid id)
        => _brick.Contains(id);

    public override bool ContainsAny()
        => _singleton != null;

    private Guid? ResetSingleton()
    {
        if (_entityIds.Count != 0) {
            _singleton = _entityIds.First();
        }
        return _singleton;
    }

    private void ClearBlock(ref FastHashBrick<Guid, TStoredComponent>.Block block, Guid id)
    {
        block.Value = default!;
        _entityIds.Remove(id);
        if (_singleton == id) {
            _singleton = null;
        }
    }

    public override bool Remove(Guid id)
    {
        ref var block = ref _brick.RemoveBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            return false;
        }
        ClearBlock(ref block, id);
        return true;
    }

    public override bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        ref var block = ref _brick.RemoveBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            component = default;
            return false;
        }
        component = block.Value;
        ClearBlock(ref block, id);
        return true;
    }

    public override ref TStoredComponent Set(Guid id, in TStoredComponent component)
    {
        ref var block = ref _brick.AcquireBlock(id, out _existsTemp);
        if (!_existsTemp) {
            _entityIds.Add(id);
        }
        block.Value = component;
        return ref block.Value;
    }

    public override Guid? Singleton()
        => _singleton == null ? ResetSingleton() : _singleton;

    public override IEnumerable<Guid> Query()
        => _entityIds;

    public override int GetCount()
        => _entityIds.Count;

    public override IEnumerable<object> GetAll(Guid id)
    {
        ref var block = ref _brick.FindBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(block.Value!, 1);
    }

    public override void Clear(Guid id)
        => Remove(id);

    public override void Clear()
    {
        _brick.Clear();
        _entityIds.Clear();
        _singleton = null;
    }
}

public class MonoPoolStorage<TStoredComponent> : MonoPoolStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, new()
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
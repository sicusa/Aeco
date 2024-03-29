namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public class MonoClosedHashStorage<TComponent, TStoredComponent>
    : MonoStorageBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    public int BrickCapacity { get; set; }

    private SortedSet<uint> _ids = new();
    private ClosedHashBrick<uint, TStoredComponent> _brick;

    private uint? _singleton;

    public MonoClosedHashStorage(int brickCapacity = MonoClosedHashStorage.DefaultBrickCapacity)
    {
        BrickCapacity = brickCapacity;
        _brick = new(brickCapacity);
    }

    public override bool TryGet(uint id, [MaybeNullWhen(false)] out TStoredComponent component)
        => _brick.TryGetValue(id, out component);

    public override ref TStoredComponent RequireOrNullRef(uint id)
    {
        ref var block = ref _brick.FindBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            return ref Unsafe.NullRef<TStoredComponent>();
        }
        return ref block.Value;
    }
    
    public override ref TStoredComponent Acquire(uint id)
        => ref Acquire(id, out bool _);

    public override ref TStoredComponent Acquire(uint id, out bool exists)
    {
        ref var block = ref _brick.AcquireBlock(id, out exists);
        if (!exists) {
            block.Value = new();
            _ids.Add(id);
        }
        return ref block.Value;
    }

    public override bool Contains(uint id)
        => _brick.Contains(id);

    public override bool ContainsAny()
        => _ids.Count != 0;

    private uint? ResetSingleton()
    {
        if (_ids.Count != 0) {
            _singleton = _ids.First();
        }
        return _singleton;
    }

    private void ClearBlock(ref ClosedHashBrick<uint, TStoredComponent>.Block block, uint id)
    {
        block.Value = default!;
        _ids.Remove(id);
        if (_singleton == id) {
            _singleton = null;
        }
    }

    public override bool Remove(uint id)
    {
        ref var block = ref _brick.RemoveBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            return false;
        }
        ClearBlock(ref block, id);
        return true;
    }

    public override bool Remove(uint id, [MaybeNullWhen(false)] out TStoredComponent component)
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

    public override ref TStoredComponent Set(uint id, in TStoredComponent component)
    {
        ref var block = ref _brick.AcquireBlock(id, out bool exists);
        if (!exists) {
            _ids.Add(id);
        }
        block.Value = component;
        return ref block.Value;
    }

    public override uint? Singleton()
        => _singleton == null ? ResetSingleton() : _singleton;

    public override IEnumerable<uint> Query()
        => _ids;

    public override int GetCount()
        => _ids.Count;

    public override IEnumerable<object> GetAll(uint id)
    {
        ref var block = ref _brick.FindBlock(id);
        if (Unsafe.IsNullRef(ref block)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(block.Value!, 1);
    }

    public override void Clear(uint id)
        => Remove(id);

    public override void Clear()
    {
        _brick.Clear();
        _ids.Clear();
        _singleton = null;
    }
}

public class MonoClosedHashStorage<TStoredComponent> : MonoClosedHashStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, new()
{
}

public static class MonoClosedHashStorage
{
    public const int DefaultBrickCapacity = 521;

    public class Factory<TComponent> : IDataLayerFactory<TComponent>
    {
        public static Factory<TComponent> Default =
            new() { BrickCapacity = DefaultBrickCapacity };

        public required int BrickCapacity { get; init; }

        public IBasicDataLayer<TComponent> Create<TStoredComponent>()
            where TStoredComponent : TComponent, new()
            => new MonoClosedHashStorage<TComponent, TStoredComponent>(BrickCapacity);
    }
}
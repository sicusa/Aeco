namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public class MonoPoolStorage<TComponent, TStoredComponent> : LocalMonoDataLayerBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, IDisposable, new()
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Block
    {
        public Guid Id;
        public int NextBlockIndex;
        public TStoredComponent Data;
    }

    private class Brick
    {
        public Block[] Blocks;
        public Brick? NextBrick;

        public Brick(int capacity)
        {
            Blocks = new Block[capacity];
            for (int i = 0; i < Blocks.Length; ++i) {
                ref var block = ref Blocks[i];
                block.NextBlockIndex = -1;
                block.Id = Guid.Empty;
                block.Data = new();
            }
        }

        public ref Block FindBlock(int index, Guid id)
        {
            ref var block = ref Blocks[index];

            if (block.Id == Guid.Empty) {
                if (block.NextBlockIndex == -1) {
                    return ref Unsafe.NullRef<Block>();
                }
                else if (block.NextBlockIndex == -2) {
                    return ref NextBrick!.FindBlock(index, id);
                }
            }

            int initialIndex = index;
            while (true) {
                if (block.Id == id) {
                    return ref block;
                }
                index = block.NextBlockIndex;
                if (index == -1) {
                    return ref Unsafe.NullRef<Block>();
                }
                else if (index == -2) {
                    return ref NextBrick!.FindBlock(initialIndex, id);
                }
                block = ref Blocks[index];
            }
        }

        public ref Block AcquireBlock(int index, Guid id, out bool exists)
        {
            ref var block = ref Blocks[index];

            if (block.Id == Guid.Empty) {
                block.Id = id;
                exists = false;
                return ref block;
            }

            int initialIndex = index;
            while (true) {
                if (block.Id == id) {
                    exists = true;
                    return ref block;
                }

                index = block.NextBlockIndex;
                if (index == -1) {
                    int bucketIndex = Blocks.Length - 1;
                    while (bucketIndex >= 0 && Blocks[bucketIndex].Id != Guid.Empty) {
                        --bucketIndex;
                    }
                    if (bucketIndex == -1) {
                        NextBrick ??= new Brick(Blocks.Length);
                        block.NextBlockIndex = -2;
                        return ref NextBrick.AcquireBlock(initialIndex, id, out exists);
                    }

                    ref var bucket = ref Blocks[bucketIndex];
                    bucket.Id = id;
                    block.NextBlockIndex = bucketIndex;

                    exists = false;
                    return ref bucket;
                }
                else if (index == -2) {
                    return ref NextBrick!.AcquireBlock(initialIndex, id, out exists);
                }
                block = ref Blocks[index];
            }
        }

        public ref Block RemoveBlock(int index, Guid id)
        {
            ref var block = ref Blocks[index];
            if (block.Id == Guid.Empty) {
                if (block.NextBlockIndex == -1) {
                    return ref Unsafe.NullRef<Block>();
                }
                else if (block.NextBlockIndex == -2) {
                    return ref NextBrick!.RemoveBlock(index, id);
                }
            }
            else if (block.Id == id) {
                block.Id = Guid.Empty;
                return ref block;
            }

            int prevIndex;
            int initialIndex = index;

            while (true) {
                prevIndex = index;
                index = block.NextBlockIndex;
                if (index == -1) {
                    return ref Unsafe.NullRef<Block>();
                }
                else if (index == -2) {
                    return ref NextBrick!.RemoveBlock(initialIndex, id);
                }
                block = ref Blocks[index];
                if (block.Id == id) {
                    Blocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                    block.NextBlockIndex = -1;
                    block.Id = Guid.Empty;
                    return ref block;
                }
            }
        }
    }

    public int BrickCapacity { get; set; }

    private Brick _brick;
    private SortedSet<Guid> _entityIds = new();

    private int _cellarCount;
    private Guid _singleton = Guid.Empty;
    private bool _existsTemp;

    private const int kLower31BitMask = 0x7FFFFFFF;

    public MonoPoolStorage(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
    {
        BrickCapacity = brickCapacity;
        _cellarCount = (int)(0.86 * brickCapacity);
        _brick = new Brick(brickCapacity);
    }
    
    private int GetIndex(Guid entityId)
        => (entityId.GetHashCode() & kLower31BitMask) % _cellarCount;

    public override bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.FindBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            component = default;
            return false;
        }
        component = block.Data;
        return true;
    }

    public override ref TStoredComponent Require(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.FindBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref block.Data;
    }
    
    public override ref TStoredComponent Acquire(Guid entityId)
        => ref Acquire(entityId, out _existsTemp);

    public override ref TStoredComponent Acquire(Guid entityId, out bool exists)
    {
        ref var block = ref _brick.AcquireBlock(GetIndex(entityId), entityId, out exists);
        if (!exists) {
            _entityIds.Add(entityId);
            if (_singleton == Guid.Empty) {
                _singleton = entityId;
            }
        }
        return ref block.Data;
    }

    public override bool Contains(Guid entityId)
        => _entityIds.Contains(entityId);

    public override bool ContainsAny()
        => _singleton != Guid.Empty;

    private void ResetSingleton()
    {
        _singleton = _entityIds.Count != 0 ? _entityIds.First() : Guid.Empty;
    }

    private void ClearBlock(ref Block block, in Guid entityId)
    {
        block.Data.Dispose();
        _entityIds.Remove(entityId);
        if (_singleton == entityId) {
            ResetSingleton();
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
        component = block.Data;
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
        block.Data = component;
        return ref block.Data;
    }

    public override Guid Singleton()
        => _singleton != Guid.Empty ? _singleton
            : throw new KeyNotFoundException("Singleton not found");

    public override IEnumerable<Guid> Query()
        => _entityIds;

    public override IEnumerable<object> GetAll(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _brick.FindBlock(index, entityId);
        if (Unsafe.IsNullRef(ref block)) {
            return Enumerable.Empty<object>();
        }
        return Enumerable.Repeat<object>(block.Data, 1);
    }

    public override void Clear(Guid entityId)
        => Remove(entityId);

    public override void Clear()
    {
        var brick = _brick;
        while (brick != null) {
            var blocks = brick.Blocks;
            for (int i = 0; i < blocks.Length; ++i) {
                ref var block = ref blocks[i];
                block.NextBlockIndex = -1;
                if (block.Id != Guid.Empty) {
                    block.Id = Guid.Empty;
                    block.Data.Dispose();
                }
            }
            brick = brick.NextBrick;
        }
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
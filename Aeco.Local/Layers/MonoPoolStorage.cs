namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

public class MonoPoolStorage<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable, new()
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Block<T>
    {
        public Guid Id = Guid.Empty;
        public int NextBlockIndex = -1;
        [AllowNull]
        public T Data = default;
        public Block() {}
    }

    public int Capacity => _blocks.Length;

    private Block<TSelectedComponent>[] _blocks;
    private SortedSet<Guid> _entityIds = new();

    private int _cellarCount;
    private Guid _singleton = Guid.Empty;
    private bool _existsTemp;

    private const int kLower31BitMask = 0x7FFFFFFF;

    public MonoPoolStorage(int capacity = MonoPoolStorage.DefaultCapacity)
    {
        _blocks = new Block<TSelectedComponent>[capacity];
        _cellarCount = (int)(0.86 * capacity);

        if (default(TSelectedComponent) == null) {
            for (int i = 0; i != _blocks.Length; ++i) {
                _blocks[i].Data = new TSelectedComponent();
            }
        }
    }

    public override bool CheckSupported(Type componentType)
        => typeof(TSelectedComponent) == componentType;
    
    private int GetIndex(Guid entityId)
        => (entityId.GetHashCode() & kLower31BitMask) % _cellarCount;

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var convertedBlocks = _blocks as Block<UComponent>[]
            ?? throw new NotSupportedException("Component not supported");

        int index = GetIndex(entityId);
        ref var block = ref convertedBlocks[index];

        if (block.Id == Guid.Empty) {
            component = default;
            return false;
        }

        while (true) {
            if (block.Id == entityId) {
                component = block.Data;
                return true;
            }
            index = block.NextBlockIndex;
            if (index == -1) {
                component = default;
                return false;
            }
            block = ref convertedBlocks[index];
        }
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        var convertedBlocks = _blocks as Block<UComponent>[]
            ?? throw new NotSupportedException("Component not supported");

        int index = GetIndex(entityId);
        ref var block = ref convertedBlocks[index];

        if (block.Id == Guid.Empty) {
            throw new KeyNotFoundException("Component not found");
        }

        while (true) {
            if (block.Id == entityId) {
                return ref block.Data;
            }
            index = block.NextBlockIndex;
            if (index == -1) {
                throw new KeyNotFoundException("Component not found");
            }
            block = ref convertedBlocks[index];
        }
    }

    private ref Block<UComponent> AcquireBlock<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent
    {
        var convertedBlocks = _blocks as Block<UComponent>[]
            ?? throw new NotSupportedException("Component not supported");

        int index = GetIndex(entityId);
        ref var block = ref convertedBlocks[index];

        if (block.Id == Guid.Empty) {
            block.Id = entityId;
            _entityIds.Add(entityId);
            if (_singleton == Guid.Empty) {
                _singleton = entityId;
            }
            exists = false;
            return ref block;
        }

        while (true) {
            if (block.Id == entityId) {
                exists = true;
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == -1) {
                int bucketIndex = convertedBlocks.Length - 1;
                while (bucketIndex > 0 && convertedBlocks[bucketIndex].Id != Guid.Empty) {
                    --bucketIndex;
                }
                if (bucketIndex == -1) {
                    throw new InvalidOperationException("PoolStorage is full");
                }
                ref var bucket = ref convertedBlocks[bucketIndex];
                bucket.Id = entityId;
                block.NextBlockIndex = bucketIndex;
                _entityIds.Add(entityId);
                if (_singleton == Guid.Empty) {
                    _singleton = entityId;
                }
                exists = false;
                return ref bucket;
            }
            block = ref convertedBlocks[index];
        }
    }
    
    public override ref UComponent Acquire<UComponent>(Guid entityId)
        => ref AcquireBlock<UComponent>(entityId, out _existsTemp).Data;

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        => ref AcquireBlock<UComponent>(entityId, out exists).Data;

    public override bool Contains<UComponent>(Guid entityId)
        => _entityIds.Contains(entityId);

    private void ResetSingleton()
    {
        _singleton = _entityIds.Count != 0 ? _entityIds.First() : Guid.Empty;
    }

    private bool RawRemove(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _blocks[index];

        if (block.Id == Guid.Empty) {
            return false;
        }

        int prevIndex = index;
        while (true) {
            if (block.Id == entityId) {
                _blocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                block.Id = Guid.Empty;
                block.Data.Dispose();
                _entityIds.Remove(entityId);
                if (_singleton == entityId) {
                    ResetSingleton();
                }
                return true;
            }
            prevIndex = index;
            index = block.NextBlockIndex;
            if (index == -1) {
                return false;
            }
            block = ref _blocks[index];
        }
    }

    public override bool Remove<UComponent>(Guid entityId)
        => RawRemove(entityId);

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var convertedBlocks = _blocks as Block<UComponent>[]
            ?? throw new NotSupportedException("Component not supported");

        int index = GetIndex(entityId);
        ref var block = ref convertedBlocks[index];

        if (block.Id == Guid.Empty) {
            component = default;
            return false;
        }

        int prevIndex = index;
        while (true) {
            if (block.Id == entityId) {
                convertedBlocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                component = block.Data;

                block.Id = Guid.Empty;
                _blocks[index].Data.Dispose();

                _entityIds.Remove(entityId);
                if (_singleton == entityId) {
                    ResetSingleton();
                }
                return true;
            }
            prevIndex = index;
            index = block.NextBlockIndex;
            if (index == -1) {
                component = default;
                return false;
            }
            block = ref convertedBlocks[index];
        }
    }

    public override void Set<UComponent>(Guid entityId, in UComponent component)
    {
        ref var block = ref AcquireBlock<UComponent>(entityId, out var _);
        block.Data = component;
    }

    public override Guid Singleton<UComponent>()
        => _singleton != Guid.Empty ? _singleton
            : throw new KeyNotFoundException("Singleton not found");

    public override IEnumerable<Guid> Query<UComponent>()
        => _entityIds;

    public override IEnumerable<object> GetAll(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _blocks[index];

        if (block.Id == Guid.Empty) {
            return Enumerable.Empty<object>();
        }

        while (true) {
            if (block.Id == entityId) {
                return Enumerable.Repeat<object>(block.Data, 1);
            }
            index = block.NextBlockIndex;
            if (index == -1) {
                return Enumerable.Empty<object>();
            }
            block = ref _blocks[index];
        }
    }

    public override void Clear(Guid entityId)
        => RawRemove(entityId);

    public override void Clear()
    {
        _blocks = new Block<TSelectedComponent>[_blocks.Length];
        if (default(TSelectedComponent) == null) {
            for (int i = 0; i != _blocks.Length; ++i) {
                _blocks[i].Data = new TSelectedComponent();
            }
        }
    }
}

public class MonoPoolStorage<TSelectedComponent> : MonoPoolStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable, new()
{
}

public static class MonoPoolStorage
{
    public const int DefaultCapacity = 256;

    public static IDataLayer<TComponent> CreateUnsafe<TComponent>(Type selectedComponentType, int capacity = DefaultCapacity)
    {
        var type = typeof(MonoPoolStorage<,>).MakeGenericType(
            new Type[] {typeof(TComponent), selectedComponentType});
        return (IDataLayer<TComponent>)Activator.CreateInstance(type, new object[] {capacity})!;
    }

    public static Func<Type, IDataLayer<TComponent>> MakeUnsafeCreator<TComponent>(int capacity = DefaultCapacity)
        => selectedComponentType =>
            CreateUnsafe<TComponent>(selectedComponentType, capacity);
}
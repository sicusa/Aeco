namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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

    public int Capacity => _blocks.Length;

    private Block[] _blocks;
    private SortedSet<Guid> _entityIds = new();

    private int _cellarCount;
    private Guid _singleton = Guid.Empty;
    private bool _existsTemp;

    private const int kLower31BitMask = 0x7FFFFFFF;

    public MonoPoolStorage(int capacity = MonoPoolStorage.DefaultCapacity)
    {
        _blocks = new Block[capacity];
        _cellarCount = (int)(0.86 * capacity);
        for (int i = 0; i < _blocks.Length; ++i) {
            _blocks[i].NextBlockIndex = -1;
            _blocks[i].Data = new();
        }
    }
    
    private int GetIndex(Guid entityId)
        => (entityId.GetHashCode() & kLower31BitMask) % _cellarCount;

    public override bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        int index = GetIndex(entityId);
        ref var block = ref _blocks[index];

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
            block = ref _blocks[index];
        }
    }

    public override ref TStoredComponent Require(Guid entityId)
    {
        int index = GetIndex(entityId);
        ref var block = ref _blocks[index];

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
            block = ref _blocks[index];
        }
    }

    private ref Block AcquireBlock(Guid entityId, out bool exists)
    {
        int index = GetIndex(entityId);
        ref var block = ref _blocks[index];

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
                int bucketIndex = _blocks.Length - 1;
                while (bucketIndex > 0 && _blocks[bucketIndex].Id != Guid.Empty) {
                    --bucketIndex;
                }
                if (bucketIndex == -1) {
                    throw new InvalidOperationException("PoolStorage is full");
                }

                ref var bucket = ref _blocks[bucketIndex];
                bucket.Id = entityId;
                block.NextBlockIndex = bucketIndex;

                _entityIds.Add(entityId);
                if (_singleton == Guid.Empty) {
                    _singleton = entityId;
                }
                exists = false;
                return ref bucket;
            }
            block = ref _blocks[index];
        }
    }
    
    public override ref TStoredComponent Acquire(Guid entityId)
        => ref AcquireBlock(entityId, out _existsTemp).Data;

    public override ref TStoredComponent Acquire(Guid entityId, out bool exists)
        => ref AcquireBlock(entityId, out exists).Data;

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
                block.NextBlockIndex = -1;

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

    public override bool Remove(Guid entityId)
        => RawRemove(entityId);

    public override bool Remove(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        int index = GetIndex(entityId);
        ref var block = ref _blocks[index];

        if (block.Id == Guid.Empty) {
            component = default;
            return false;
        }

        int prevIndex = index;
        while (true) {
            if (block.Id == entityId) {
                _blocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                component = block.Data;

                block.Id = Guid.Empty;
                block.Data.Dispose();
                block.NextBlockIndex = -1;

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
            block = ref _blocks[index];
        }
    }

    public override ref TStoredComponent Set(Guid entityId, in TStoredComponent component)
    {
        ref var block = ref AcquireBlock(entityId, out _existsTemp);
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
        for (int i = 0; i < _blocks.Length; ++i) {
            ref var block = ref _blocks[i];
            if (block.Id != Guid.Empty) {
                _blocks[i].Id = Guid.Empty;
                _blocks[i].Data.Dispose();
                _blocks[i].NextBlockIndex = -1;
            }
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
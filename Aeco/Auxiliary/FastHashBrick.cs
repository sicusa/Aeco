namespace Aeco;

using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class FastHashBrick<TKey, TValue>
    where TKey : notnull, IEquatable<TKey>
{
    public struct Block
    {
        public TKey Key;
        public TValue Value;
        public int NextBlockIndex;
    }

    public int Capacity { get; private set; }
    public int CellarCapacity { get; private set; }
    public int UsedBlockCount { get; private set; }

    private Block[] _blocks;
    private FastHashBrick<TKey, TValue>? _nextBrick;

    public FastHashBrick(int capacity)
    {
        Capacity = capacity;
        CellarCapacity = FastHashBrick.CalculateProperCellarCapacity(capacity);
        _blocks = new Block[capacity];
    }
    
    private int GetIndex(TKey key)
        => FastHashBrick.CalculateIndex(key.GetHashCode(), CellarCapacity);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (key.Equals(default)) {
            value = default;
            return false;
        }
        return TryGetValue(GetIndex(key), key, out value);
    }

    private bool TryGetValue(int index, TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        int initialIndex = index;
        ref var block = ref _blocks[index];

        while (true) {
            if (block.Key.Equals(key)) {
                value = block.Value;
                return true;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                value = default;
                return false;
            }
            else if (index == -1) {
                return _nextBrick!.TryGetValue(initialIndex, key, out value);
            }
            block = ref _blocks[index == -2 ? 0 : index];
        }
    }

    public ref Block FindBlock(TKey key)
    {
        if (key.Equals(default)) {
            return ref Unsafe.NullRef<Block>();
        }
        return ref FindBlock(GetIndex(key), key);
    }

    private ref Block FindBlock(int index, TKey key)
    {
        int initialIndex = index;
        ref var block = ref _blocks[index];

        while (true) {
            if (block.Key.Equals(key)) {
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                return ref Unsafe.NullRef<Block>();
            }
            else if (index == -1) {
                return ref _nextBrick!.FindBlock(initialIndex, key);
            }
            block = ref _blocks[index == -2 ? 0 : index];
        }
    }

    public bool Contains(TKey key)
    {
        if (key.Equals(default)) {
            return false;
        }
        return Contains(GetIndex(key), key);
    }

    private bool Contains(int index, TKey key)
    {
        int initialIndex = index;
        ref var block = ref _blocks[index];

        while (true) {
            if (block.Key.Equals(key)) {
                return true;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                return false;
            }
            else if (index == -1) {
                return _nextBrick!.Contains(initialIndex, key);
            }
            block = ref _blocks[index == -2 ? 0 : index];
        }
    }

    private bool IsBlockInUse(ref Block block)
        => !block.Key.Equals(default) || block.NextBlockIndex != 0;

    public ref Block AcquireBlock(TKey key, out bool exists)
    {
        if (key.Equals(default)) {
            throw new InvalidOperationException("Key cannot be default value");
        }
        return ref AcquireBlock(GetIndex(key), key, out exists);
    }

    private ref Block AcquireBlock(int index, TKey key, out bool exists)
    {
        ref var block = ref _blocks[index];
        int initialIndex = index;
        int emptyBlockIndex = -1;

        while (true) {
            if (emptyBlockIndex == -1 && block.Key.Equals(default)) {
                emptyBlockIndex = index;
            }
            if (block.Key.Equals(key)) {
                exists = true;
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                if (emptyBlockIndex != -1) {
                    block = ref _blocks[emptyBlockIndex == -2 ? 0 : emptyBlockIndex];
                    block.Key = key;
                    exists = false;
                    if (block.NextBlockIndex == 0) {
                        ++UsedBlockCount;
                    }
                    return ref block;
                }

                if (UsedBlockCount >= Capacity) {
                    block.NextBlockIndex = -1;
                    _nextBrick ??= new(Capacity);
                    return ref _nextBrick.AcquireBlock(initialIndex, key, out exists);
                }

                int bucketIndex = Capacity - 1;
                while (IsBlockInUse(ref _blocks[bucketIndex])) {
                    --bucketIndex;
                }

                ref var bucket = ref _blocks[bucketIndex];
                bucket.Key = key;
                block.NextBlockIndex = bucketIndex == 0 ? -2 : bucketIndex;

                ++UsedBlockCount;
                exists = false;
                return ref bucket;
            }
            else if (index == -1) {
                if (emptyBlockIndex == -1 && UsedBlockCount >= Capacity) {
                    return ref _nextBrick!.AcquireBlock(initialIndex, key, out exists);
                }

                ref var foundBlock = ref _nextBrick!.FindBlock(initialIndex, key);
                if (!Unsafe.IsNullRef(ref foundBlock)) {
                    exists = true;
                    return ref foundBlock;
                }

                if (emptyBlockIndex != -1) {
                    block = ref _blocks[emptyBlockIndex == -2 ? 0 : emptyBlockIndex];
                    block.Key = key;
                    exists = false;
                    return ref block;
                }

                int bucketIndex = Capacity - 1;
                while (IsBlockInUse(ref _blocks[bucketIndex])) {
                    --bucketIndex;
                }

                ref var bucket = ref _blocks[bucketIndex];
                bucket.Key = key;
                bucket.NextBlockIndex = -1;
                block.NextBlockIndex = bucketIndex == 0 ? -2 : bucketIndex;

                ++UsedBlockCount;
                exists = false;
                return ref bucket;
            }
            block = ref _blocks[index == -2 ? 0 : index];
        }
    }

    public ref Block RemoveBlock(TKey key)
    {
        if (key.Equals(default)) {
            return ref Unsafe.NullRef<Block>();
        }
        return ref RemoveBlock(GetIndex(key), key);
    }

    private ref Block RemoveBlock(int index, TKey key)
    {
        ref var block = ref _blocks[index];
        if (block.Key.Equals(key)) {
            block.Key = default!;
            if (block.NextBlockIndex == 0) {
                --UsedBlockCount;
            }
            return ref block;
        }

        int prevIndex;
        int initialIndex = index;

        while (true) {
            prevIndex = index;
            index = block.NextBlockIndex;

            if (index == 0) {
                return ref Unsafe.NullRef<Block>();
            }
            else if (index == -1) {
                return ref _nextBrick!.RemoveBlock(initialIndex, key);
            }

            block = ref _blocks[index == -2 ? 0 : index];

            if (block.Key.Equals(key)) {
                if (index >= CellarCapacity) {
                    _blocks[prevIndex == -2 ? 0 : prevIndex].NextBlockIndex = block.NextBlockIndex;
                    block.NextBlockIndex = 0;
                    --UsedBlockCount;
                }
                block.Key = default!;
                return ref block;
            }
        }
    }

    public void Clear()
    {
        if (UsedBlockCount != 0) {
            Array.Clear(_blocks);
            UsedBlockCount = 0;
        }
        _nextBrick?.Clear();
    }
}

public static class FastHashBrick
{
    public const int Lower31BitMask = 0x7FFFFFFF;

    public static int CalculateProperCellarCapacity(int brickCapacity)
        => (int)(0.86f * brickCapacity);
    
    public static int CalculateIndex(int hashCode, int cellarCapacity)
        => (hashCode & Lower31BitMask) % cellarCapacity;
}
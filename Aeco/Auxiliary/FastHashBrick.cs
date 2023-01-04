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
    public int Count { get; private set; }

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
        => TryGetValue(GetIndex(key), key, out value);

    private bool TryGetValue(int index, TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (key.Equals(default)) {
            value = default;
            return false;
        }

        int initialIndex = index;
        ref var block = ref _blocks[index];

        while (true) {
            if (block.Key.Equals(key)) {
                value = block.Value;
                return true;
            }
            index = block.NextBlockIndex;

            if (index == 0) {
                if (_nextBrick != null && _nextBrick.Count > 0) {
                    return _nextBrick.TryGetValue(index, key, out value);
                }
                value = default;
                return false;
            }
            block = ref _blocks[index == -1 ? 0 : index];
        }
    }

    public ref Block FindBlock(TKey key)
        => ref FindBlock(GetIndex(key), key);

    private ref Block FindBlock(int index, TKey key)
    {
        if (key.Equals(default)) {
            return ref Unsafe.NullRef<Block>();
        }

        int initialIndex = index;
        ref var block = ref _blocks[index];

        while (true) {
            if (block.Key.Equals(key)) {
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                if (_nextBrick != null && _nextBrick.Count > 0) {
                    return ref _nextBrick.FindBlock(initialIndex, key);
                }
                return ref Unsafe.NullRef<Block>();
            }
            block = ref _blocks[index == -1 ? 0 : index];
        }
    }

    public bool Contains(TKey key)
        => Contains(GetIndex(key), key);

    private bool Contains(int index, TKey key)
    {
        if (key.Equals(default)) {
            return false;
        }

        int initialIndex = index;
        ref var block = ref _blocks[index];

        while (true) {
            if (block.Key.Equals(key)) {
                return true;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                if (_nextBrick != null && _nextBrick.Count > 0) {
                    return _nextBrick.Contains(initialIndex, key);
                }
                return false;
            }
            block = ref _blocks[index == -1 ? 0 : index];
        }
    }

    private bool IsBlockInUse(ref Block block)
        => !block.Key.Equals(default);

    public ref Block AcquireBlock(TKey key, out bool exists)
        => ref AcquireBlock(GetIndex(key), key, out exists);

    private ref Block AcquireBlock(int index, TKey key, out bool exists)
    {
        if (key.Equals(default)) {
            throw new InvalidOperationException("Key cannot be default value");
        }

        ref var block = ref _blocks[index];
        int initialIndex = index;

        while (true) {
            if (block.Key.Equals(default)) {
                block.Key = key;
                ++Count;
                exists = false;
                return ref block;
            }
            if (block.Key.Equals(key)) {
                exists = true;
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                if (Count >= Capacity) {
                    _nextBrick ??= new(Capacity);
                    return ref _nextBrick.AcquireBlock(initialIndex, key, out exists);
                }

                if (_nextBrick != null && _nextBrick.Count > 0) {
                    ref var foundBlock = ref _nextBrick!.FindBlock(initialIndex, key);
                    if (!Unsafe.IsNullRef(ref foundBlock)) {
                        exists = true;
                        return ref foundBlock;
                    }
                }

                int bucketIndex = Capacity - 1;
                while (IsBlockInUse(ref _blocks[bucketIndex])) {
                    --bucketIndex;
                }

                ref var bucket = ref _blocks[bucketIndex];
                bucket.Key = key;
                block.NextBlockIndex = bucketIndex == 0 ? -1 : bucketIndex;

                ++Count;
                exists = false;
                return ref bucket;
            }
            block = ref _blocks[index == -1 ? 0 : index];
        }
    }

    public ref Block RemoveBlock(TKey key)
        => ref RemoveBlock(GetIndex(key), key);

    private ref Block RemoveBlock(int index, TKey key)
    {
        if (key.Equals(default)) {
            return ref Unsafe.NullRef<Block>();
        }

        ref var block = ref _blocks[index];
        if (block.Key.Equals(key)) {
            block.Key = default!;
            --Count;
            return ref block;
        }

        int prevIndex;
        int initialIndex = index;

        while (true) {
            prevIndex = index;
            index = block.NextBlockIndex;

            if (index == 0) {
                if (_nextBrick != null && _nextBrick.Count > 0) {
                    return ref _nextBrick!.RemoveBlock(initialIndex, key);
                }
                return ref Unsafe.NullRef<Block>();
            }

            block = ref _blocks[index == -1 ? 0 : index];

            if (block.Key.Equals(key)) {
                _blocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                if (index >= CellarCapacity) {
                    block.NextBlockIndex = 0;
                }
                block.Key = default!;
                return ref block;
            }
        }
    }

    public void Clear()
    {
        if (Count != 0) {
            Array.Clear(_blocks);
            Count = 0;
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
namespace Aeco;

using System.Buffers;
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

    private Block[] _blocks;
    private FastHashBrick<TKey, TValue>? _nextBrick;

    public FastHashBrick(int capacity)
    {
        Capacity = capacity;
        _blocks = ArrayPool<Block>.Shared.Rent(capacity);
        Array.Clear(_blocks);
    }

    ~FastHashBrick()
    {
        ArrayPool<Block>.Shared.Return(_blocks);
    }

    public bool TryGetValue(int index, TKey key, [MaybeNullWhen(false)] out TValue value)
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
            block = ref _blocks[index];
        }
    }

    public ref Block FindBlock(int index, TKey key)
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
            block = ref _blocks[index];
        }
    }

    public ref Block AcquireBlock(int index, TKey key, out bool exists)
    {
        if (key.Equals(default)) {
            throw new InvalidOperationException("Key cannot be default value");
        }
        ref var block = ref _blocks[index];
        int initialIndex = index;

        while (true) {
            if (block.Key.Equals(key)) {
                exists = true;
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                if (block.Key.Equals(default)) {
                    block.Key = key;
                    exists = false;
                    return ref block;
                }

                int bucketIndex = _blocks.Length - 1;
                while (bucketIndex > 0 && !_blocks[bucketIndex].Key.Equals(default)) {
                    --bucketIndex;
                }
                if (bucketIndex == 0) {
                    _nextBrick ??= new(Capacity);
                    block.NextBlockIndex = -1;
                    return ref _nextBrick.AcquireBlock(initialIndex, key, out exists);
                }

                ref var bucket = ref _blocks[bucketIndex];
                bucket.Key = key;
                block.NextBlockIndex = bucketIndex;

                exists = false;
                return ref bucket;
            }
            else if (index == -1) {
                return ref _nextBrick!.AcquireBlock(initialIndex, key, out exists);
            }
            block = ref _blocks[index];
        }
    }

    public ref Block RemoveBlock(int index, TKey key)
    {
        ref var block = ref _blocks[index];
        if (block.Key.Equals(key)) {
            block.Key = default!;
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
            block = ref _blocks[index];
            if (block.Key.Equals(key)) {
                _blocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                block.NextBlockIndex = 0;
                block.Key = default!;
                return ref block;
            }
        }
    }

    public void Clear()
    {
        Array.Clear(_blocks);
        _nextBrick = null;
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
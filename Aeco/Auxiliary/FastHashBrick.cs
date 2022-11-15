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

    public volatile Block[] Blocks;
    public volatile FastHashBrick<TKey, TValue>? NextBrick;

    public FastHashBrick(int capacity)
    {
        Blocks = new Block[capacity];
    }

    public bool TryGetValue(int index, TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref var block = ref Blocks[index];

        if (block.Key.Equals(default)) {
            if (block.NextBlockIndex == 0) {
                value = default;
                return false;
            }
            else if (block.NextBlockIndex == -1) {
                return NextBrick!.TryGetValue(index, key, out value);
            }
        }

        int initialIndex = index;
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
                return NextBrick!.TryGetValue(initialIndex, key, out value);
            }
            block = ref Blocks[index];
        }
    }

    public ref Block FindBlock(int index, TKey key)
    {
        ref var block = ref Blocks[index];

        if (block.Key.Equals(default)) {
            if (block.NextBlockIndex == 0) {
                return ref Unsafe.NullRef<Block>();
            }
            else if (block.NextBlockIndex == -1) {
                return ref NextBrick!.FindBlock(index, key);
            }
        }

        int initialIndex = index;
        while (true) {
            if (block.Key.Equals(key)) {
                return ref block;
            }
            index = block.NextBlockIndex;
            if (index == 0) {
                return ref Unsafe.NullRef<Block>();
            }
            else if (index == -1) {
                return ref NextBrick!.FindBlock(initialIndex, key);
            }
            block = ref Blocks[index];
        }
    }

    public ref Block AcquireBlock(int index, TKey key, out bool exists)
    {
        ref var block = ref Blocks[index];

        if (block.Key.Equals(default)) {
            block.Key = key;
            exists = false;
            return ref block;
        }

        int initialIndex = index;
        while (true) {
            if (block.Key.Equals(key)) {
                exists = true;
                return ref block;
            }

            index = block.NextBlockIndex;
            if (index == 0) {
                int bucketIndex = Blocks.Length - 1;
                while (bucketIndex > 0 && !Blocks[bucketIndex].Key.Equals(default)) {
                    --bucketIndex;
                }
                if (bucketIndex == 0) {
                    NextBrick ??= new(Blocks.Length);
                    block.NextBlockIndex = -1;
                    return ref NextBrick.AcquireBlock(initialIndex, key, out exists);
                }

                ref var bucket = ref Blocks[bucketIndex];
                bucket.Key = key;
                block.NextBlockIndex = bucketIndex;

                exists = false;
                return ref bucket;
            }
            else if (index == -1) {
                return ref NextBrick!.AcquireBlock(initialIndex, key, out exists);
            }
            block = ref Blocks[index];
        }
    }

    public ref Block RemoveBlock(int index, TKey key)
    {
        ref var block = ref Blocks[index];
        if (block.Key.Equals(default)) {
            if (block.NextBlockIndex == 0) {
                return ref Unsafe.NullRef<Block>();
            }
            else if (block.NextBlockIndex == -1) {
                return ref NextBrick!.RemoveBlock(index, key);
            }
        }
        else if (block.Key.Equals(key)) {
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
                return ref NextBrick!.RemoveBlock(initialIndex, key);
            }
            block = ref Blocks[index];
            if (block.Key.Equals(key)) {
                Blocks[prevIndex].NextBlockIndex = block.NextBlockIndex;
                block.NextBlockIndex = 0;
                block.Key = default!;
                return ref block;
            }
        }
    }

    public void Clear()
    {
        var brick = this;
        while (brick != null) {
            Array.Clear(brick.Blocks);
            brick = brick.NextBrick;
        }
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
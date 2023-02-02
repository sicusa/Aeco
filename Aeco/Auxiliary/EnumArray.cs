namespace Aeco;

using System.Collections;

public sealed class EnumArray<TKey, TElement>
    : IEnumerable<TElement>, ICollection<TElement>, IReadOnlyCollection<TElement>, IEnumerable
    , IStructuralComparable, IStructuralEquatable, ICloneable
    where TKey : Enum
{
    public TElement[] Raw => _array;

    private readonly TElement[] _array;

    private static int s_lower;
    private static int s_upper;

    static EnumArray()
    {
        var values = Enum.GetValues(typeof(TKey)).Cast<TKey>();
        s_lower = Convert.ToInt32(values.Min());
        s_upper = Convert.ToInt32(values.Max());
    }

    public EnumArray()
    {
        _array = new TElement[1 + s_upper - s_lower];
    }

    public EnumArray(IEnumerable<TElement> elements)
        : this()
    {
        int i = 0;
        foreach (var elem in elements) {
            if (i >= _array.Length) {
                return;
            }
            _array[i] = elem;
            ++i;
        }
    }

    public EnumArray(IEnumerable<KeyValuePair<TKey, TElement>> pairs)
        : this()
    {
        foreach (var pair in pairs) {
            _array[Convert.ToInt32(pair.Key)] = pair.Value;
        }
    }

    private EnumArray(EnumArray<TKey, TElement> other)
    {
        _array = (TElement[])other._array.Clone();
    }

    public ref TElement this[TKey key]
        => ref _array[Convert.ToInt32(key) - s_lower];

    public int Length => _array.Length;

    int ICollection<TElement>.Count => ((ICollection<TElement>)_array).Count;
    int IReadOnlyCollection<TElement>.Count => ((ICollection<TElement>)_array).Count;

    public bool IsReadOnly => ((ICollection<TElement>)_array).IsReadOnly;

    public Span<TElement>.Enumerator GetEnumerator()
        => _array.AsSpan().GetEnumerator();

    IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        => ((IEnumerable<TElement>)_array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _array.GetEnumerator();

    public int CompareTo(object? other, IComparer comparer)
        => ((IStructuralComparable)_array).CompareTo(other, comparer);

    public bool Equals(object? other, IEqualityComparer comparer)
        => ((IStructuralEquatable)_array).Equals(other, comparer);

    public int GetHashCode(IEqualityComparer comparer)
        => ((IStructuralEquatable)_array).GetHashCode(comparer);

    public object Clone()
        => new EnumArray<TKey, TElement>(this);

    void ICollection<TElement>.Add(TElement item)
        => ((ICollection<TElement>)_array).Add(item);

    void ICollection<TElement>.Clear()
        => ((ICollection<TElement>)_array).Clear();

    bool ICollection<TElement>.Contains(TElement item)
        => ((ICollection<TElement>)_array).Contains(item);

    public void CopyTo(TElement[] array, int arrayIndex)
        => ((ICollection<TElement>)_array).CopyTo(array, arrayIndex);

    bool ICollection<TElement>.Remove(TElement item)
        => ((ICollection<TElement>)_array).Remove(item);
}
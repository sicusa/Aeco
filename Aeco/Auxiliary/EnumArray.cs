namespace Aeco;

using System.Collections;

public sealed class EnumArray<TKey, TElement>
    : IEnumerable<TElement>, ICollection<TElement>, IReadOnlyCollection<TElement>, IEnumerable
    , IStructuralComparable, IStructuralEquatable, ICloneable
    where TKey : Enum
{
    public TElement[] Raw => _array;

    private readonly TElement[] _array;

    private bool s_boundSet;
    private static int s_lower;
    private static int s_upper;

    public EnumArray()
    {
        if (!s_boundSet) {
            var values = Enum.GetValues(typeof(TKey)).Cast<TKey>();
            s_lower = Convert.ToInt32(values.Min());
            s_upper = Convert.ToInt32(values.Max());
            s_boundSet = true;
        }
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

    public TElement this[TKey key]
    {
        get { return _array[Convert.ToInt32(key) - s_lower]; }
        set { _array[Convert.ToInt32(key) - s_lower] = value; }
    }

    public TElement this[int index] { get => ((IList<TElement>)_array)[index]; set => ((IList<TElement>)_array)[index] = value; }

    public int Length => _array.Length;

    int ICollection<TElement>.Count => ((ICollection<TElement>)_array).Count;
    int IReadOnlyCollection<TElement>.Count => ((ICollection<TElement>)_array).Count;

    public bool IsReadOnly => ((ICollection<TElement>)_array).IsReadOnly;

    public IEnumerator<TElement> GetEnumerator()
    {
        return ((IEnumerable<TElement>)_array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _array.GetEnumerator();
    }

    public int CompareTo(object? other, IComparer comparer)
    {
        return ((IStructuralComparable)_array).CompareTo(other, comparer);
    }

    public bool Equals(object? other, IEqualityComparer comparer)
    {
        return ((IStructuralEquatable)_array).Equals(other, comparer);
    }

    public int GetHashCode(IEqualityComparer comparer)
    {
        return ((IStructuralEquatable)_array).GetHashCode(comparer);
    }

    public object Clone()
        => new EnumArray<TKey, TElement>(this);

    void ICollection<TElement>.Add(TElement item)
        => ((ICollection<TElement>)_array).Add(item);

    void ICollection<TElement>.Clear()
        => ((ICollection<TElement>)_array).Clear();

    bool ICollection<TElement>.Contains(TElement item)
        => ((ICollection<TElement>)_array).Contains(item);

    public void CopyTo(TElement[] array, int arrayIndex)
    {
        ((ICollection<TElement>)_array).CopyTo(array, arrayIndex);
    }

    bool ICollection<TElement>.Remove(TElement item)
        => ((ICollection<TElement>)_array).Remove(item);
}
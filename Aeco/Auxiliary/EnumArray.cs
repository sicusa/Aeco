namespace Aeco;

using System.Collections;

public class EnumArray<TKey, TElement>
    : IEnumerable<TElement>, ICollection<TElement>, IReadOnlyCollection<TElement>, IEnumerable
    , IStructuralComparable, IStructuralEquatable, ICloneable
    where TKey : Enum
{
    public TElement[] Raw => _array;

    private readonly TElement[] _array;
    private readonly int _lower;

    public EnumArray()
    {
        _lower = Convert.ToInt32(Enum.GetValues(typeof(TKey)).Cast<TKey>().Min());
        int upper = Convert.ToInt32(Enum.GetValues(typeof(TKey)).Cast<TKey>().Max());
        _array = new TElement[1 + upper - _lower];
    }

    public EnumArray(IEnumerable<KeyValuePair<TKey, TElement>> pairs)
        : this()
    {
        foreach (var pair in pairs) {
            _array[Convert.ToInt32(pair.Key)] = pair.Value;
        }
    }

    protected EnumArray(EnumArray<TKey, TElement> other)
    {
        _array = (TElement[])other._array.Clone();
        _lower = other._lower;
    }

    public TElement this[TKey key]
    {
        get { return _array[Convert.ToInt32(key) - _lower]; }
        set { _array[Convert.ToInt32(key) - _lower] = value; }
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
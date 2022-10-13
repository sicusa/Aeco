namespace Aeco;

using System.Collections;

public class EnumArray<TKey, TElement>
    : IEnumerable<TElement>, ICollection<TElement>, IEnumerable, IList<TElement>
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

    public TElement this[TKey key]
    {
        get { return _array[Convert.ToInt32(key) - _lower]; }
        set { _array[Convert.ToInt32(key) - _lower] = value; }
    }

    public TElement this[int index] { get => ((IList<TElement>)_array)[index]; set => ((IList<TElement>)_array)[index] = value; }

    public int Count => ((ICollection<TElement>)_array).Count;

    public bool IsReadOnly => ((ICollection<TElement>)_array).IsReadOnly;

    public void Add(TElement item)
    {
        ((ICollection<TElement>)_array).Add(item);
    }

    public void Clear()
    {
        ((ICollection<TElement>)_array).Clear();
    }

    public object Clone()
    {
        return _array.Clone();
    }

    public int CompareTo(object? other, IComparer comparer)
    {
        return ((IStructuralComparable)_array).CompareTo(other, comparer);
    }

    public bool Contains(TElement item)
    {
        return ((ICollection<TElement>)_array).Contains(item);
    }

    public void CopyTo(TElement[] array, int arrayIndex)
    {
        ((ICollection<TElement>)_array).CopyTo(array, arrayIndex);
    }

    public bool Equals(object? other, IEqualityComparer comparer)
    {
        return ((IStructuralEquatable)_array).Equals(other, comparer);
    }

    public IEnumerator<TElement> GetEnumerator()
        => ((IEnumerable<TElement>)_array).GetEnumerator();

    public int GetHashCode(IEqualityComparer comparer)
    {
        return ((IStructuralEquatable)_array).GetHashCode(comparer);
    }

    public int IndexOf(TElement item)
    {
        return ((IList<TElement>)_array).IndexOf(item);
    }

    public void Insert(int index, TElement item)
    {
        ((IList<TElement>)_array).Insert(index, item);
    }

    public bool Remove(TElement item)
    {
        return ((ICollection<TElement>)_array).Remove(item);
    }

    public void RemoveAt(int index)
    {
        ((IList<TElement>)_array).RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
        => _array.GetEnumerator();
}
namespace Aeco.Reactive;

using System.Collections;
using System.Runtime.InteropServices;

public interface IGroup : IList<uint>, IReadOnlyList<uint>
{
}

public abstract class GroupBase : IGroup
{
    private List<uint> _l = new();

    public uint this[int index] => _l[index];

    public int Count => _l.Count;

    public bool IsReadOnly => ((ICollection<uint>)_l).IsReadOnly;

    uint IList<uint>.this[int index] { get => ((IList<uint>)_l)[index]; set => ((IList<uint>)_l)[index] = value; }

    private bool _initialized = false;

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => _l.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _l.GetEnumerator();

    public Span<uint>.Enumerator GetEnumerator()
        => CollectionsMarshal.AsSpan(_l).GetEnumerator();
    
    public Span<uint> AsSpan()
        => CollectionsMarshal.AsSpan(_l);
    
    protected virtual void Reset(IReadableDataLayer<IComponent> dataLayer, IEnumerable<uint> ids)
    {
        _l.Clear();
        _l.AddRange(ids);
    }

    public abstract bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer);
    public abstract void Refresh(IReadableDataLayer<IComponent> dataLayer);

    public GroupBase Query(IReadableDataLayer<IComponent> dataLayer)
    {
        if (!_initialized || ShouldRefresh(dataLayer)) {
            Refresh(dataLayer);
            _initialized = true;
        }
        return this;
    }

    public int IndexOf(uint item)
        => ((IList<uint>)_l).IndexOf(item);

    public void Insert(int index, uint item)
        => ((IList<uint>)_l).Insert(index, item);

    public void RemoveAt(int index)
        => ((IList<uint>)_l).RemoveAt(index);

    public void Add(uint item)
        => ((ICollection<uint>)_l).Add(item);

    public void Clear()
        => ((ICollection<uint>)_l).Clear();

    public bool Contains(uint item)
        => ((ICollection<uint>)_l).Contains(item);

    public void CopyTo(uint[] array, int arrayIndex)
        => ((ICollection<uint>)_l).CopyTo(array, arrayIndex);

    public bool Remove(uint item)
        => ((ICollection<uint>)_l).Remove(item);
    
    public void Sort(Comparison<uint> comparision)
        => _l.Sort(comparision);

    public void Sort(IComparer<uint> comparer)
        => _l.Sort(comparer);
}

public class Group<T1> : GroupBase
    where T1 : IComponent
{
    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>();
    
    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, dataLayer.Query<T1>());
}

public class Group<T1, T2> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
{
    private Query<T1, T2> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}

public class Group<T1, T2, T3> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
{
    private Query<T1, T2, T3> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
    private Query<T1, T2, T3, T4> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4, T5> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
{
    private Query<T1, T2, T3, T4, T5> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4, T5, T6> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
{
    private Query<T1, T2, T3, T4, T5, T6> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T6>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4, T5, T6, T7> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
    where T7 : IComponent
{
    private Query<T1, T2, T3, T4, T5, T6, T7> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T6>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T7>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4, T5, T6, T7, T8> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
    where T7 : IComponent
    where T8 : IComponent
{
    private Query<T1, T2, T3, T4, T5, T6, T7, T8> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCleared>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T6>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T7>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T8>>();

    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, _q.Query(dataLayer));
}
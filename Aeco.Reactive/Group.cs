namespace Aeco.Reactive;

using System.Collections;
using System.Runtime.InteropServices;

public interface IGroup : IQuery<IComponent>, IList<Guid>, IReadOnlyList<Guid>
{
}

public abstract class GroupBase : IGroup
{
    private List<Guid> _l = new();

    public Guid this[int index] => _l[index];

    public int Count => _l.Count;

    public bool IsReadOnly => ((ICollection<Guid>)_l).IsReadOnly;

    Guid IList<Guid>.this[int index] { get => ((IList<Guid>)_l)[index]; set => ((IList<Guid>)_l)[index] = value; }

    private bool _initialized = false;

    IEnumerator<Guid> IEnumerable<Guid>.GetEnumerator()
        => _l.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _l.GetEnumerator();

    public Span<Guid>.Enumerator GetEnumerator()
        => CollectionsMarshal.AsSpan(_l).GetEnumerator();
    
    protected virtual void Reset(IReadableDataLayer<IComponent> dataLayer, IEnumerable<Guid> ids)
    {
        _l.Clear();
        _l.AddRange(ids);
    }

    public abstract bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer);
    public abstract void Refresh(IReadableDataLayer<IComponent> dataLayer);

    public IEnumerable<Guid> Query(IReadableDataLayer<IComponent> dataLayer)
    {
        if (!_initialized || ShouldRefresh(dataLayer)) {
            Refresh(dataLayer);
            _initialized = true;
        }
        return _l;
    }

    public int IndexOf(Guid item)
        => ((IList<Guid>)_l).IndexOf(item);

    public void Insert(int index, Guid item)
        => ((IList<Guid>)_l).Insert(index, item);

    public void RemoveAt(int index)
        => ((IList<Guid>)_l).RemoveAt(index);

    public void Add(Guid item)
        => ((ICollection<Guid>)_l).Add(item);

    public void Clear()
        => ((ICollection<Guid>)_l).Clear();

    public bool Contains(Guid item)
        => ((ICollection<Guid>)_l).Contains(item);

    public void CopyTo(Guid[] array, int arrayIndex)
        => ((ICollection<Guid>)_l).CopyTo(array, arrayIndex);

    public bool Remove(Guid item)
        => ((ICollection<Guid>)_l).Remove(item);
    
    public void Sort(Comparison<Guid> comparision)
        => _l.Sort(comparision);

    public void Sort(IComparer<Guid> comparer)
        => _l.Sort(comparer);
}

public class Group<T1> : GroupBase
    where T1 : IComponent
{
    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>();
    
    public override void Refresh(IReadableDataLayer<IComponent> dataLayer)
        => Reset(dataLayer, dataLayer.Query<T1>());
}

public class Group<T1, T2> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
{
    private Query<T1, T2> _q = new();

    public override bool ShouldRefresh(IReadableDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
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
using System.Collections;

namespace Aeco.Reactive;

public interface IGroup : IQuery<IComponent>, IReadOnlyList<Guid>
{
}

public abstract class GroupBase : IGroup
{
    private List<Guid> _l = new();

    public Guid this[int index] => _l[index];

    public int Count => _l.Count;

    public IEnumerator<Guid> GetEnumerator()
        => _l.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _l.GetEnumerator();
    
    protected void Reset(IEnumerable<Guid> ids)
    {
        _l.Clear();
        _l.AddRange(ids);
    }

    public abstract bool Refrash(IDataLayer<IComponent> dataLayer);

    public IEnumerable<Guid> Query(IDataLayer<IComponent> dataLayer)
    {
        Refrash(dataLayer);
        return _l;
    }
}

public class Group<T1> : GroupBase
    where T1 : IComponent
{
    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()) {
            Reset(dataLayer.Query<T1>());
            return true;
        }
        return false;
    }
}

public class Group<T1, T2> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
{
    private Query<T1, T2> _q = new();

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
}

public class Group<T1, T2, T3> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
{
    private Query<T1, T2, T3> _q = new();

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T3>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
}

public class Group<T1, T2, T3, T4> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
    private Query<T1, T2, T3, T4> _q = new();

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T3>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T4>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
}

public class Group<T1, T2, T3, T4, T5> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
{
    private Query<T1, T2, T3, T4, T5> _q = new();

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T3>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T4>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T5>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
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

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T3>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T4>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T5>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T6>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
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

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T3>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T4>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T5>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T6>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T7>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
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

    public override bool Refrash(IDataLayer<IComponent> dataLayer)
    {
        if (dataLayer.Contains<AnyCreatedOrRemoved<T1>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T2>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T3>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T4>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T5>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T6>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T7>>()
                || dataLayer.Contains<AnyCreatedOrRemoved<T8>>()) {
            Reset(_q.Query(dataLayer));
            return true;
        }
        return false;
    }
}
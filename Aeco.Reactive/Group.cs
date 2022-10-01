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

    public abstract bool ShouldRefrash(IDataLayer<IComponent> dataLayer);
    public abstract void Refrash(IDataLayer<IComponent> dataLayer);

    public IEnumerable<Guid> Query(IDataLayer<IComponent> dataLayer)
    {
        if (ShouldRefrash(dataLayer)) {
            Refrash(dataLayer);
        }
        return _l;
    }
}

public class Group<T1> : GroupBase
    where T1 : IComponent
{
    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>();
    
    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(dataLayer.Query<T1>());
}

public class Group<T1, T2> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
{
    private Query<T1, T2> _q = new();

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
}

public class Group<T1, T2, T3> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
{
    private Query<T1, T2, T3> _q = new();

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
    private Query<T1, T2, T3, T4> _q = new();

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
}

public class Group<T1, T2, T3, T4, T5> : GroupBase
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
{
    private Query<T1, T2, T3, T4, T5> _q = new();

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
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

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T6>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
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

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T6>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T7>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
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

    public override bool ShouldRefrash(IDataLayer<IComponent> dataLayer)
        => dataLayer.ContainsAny<AnyCreatedOrRemoved<T1>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T2>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T3>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T4>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T5>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T6>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T7>>()
            || dataLayer.ContainsAny<AnyCreatedOrRemoved<T8>>();

    public override void Refrash(IDataLayer<IComponent> dataLayer)
        => Reset(_q.Query(dataLayer));
}
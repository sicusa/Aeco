namespace Aeco;

using System.Diagnostics.CodeAnalysis;

public interface IQuery<out TComponent>
{
    IEnumerable<Guid> Query(IDataLayer<TComponent> dataLayer);
}

public class GenericQuery<TBase, T1, T2> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class GenericQuery<TBase, T1, T2, T3> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e3.MoveNext()) { yield break; }
                compare = e3.Current.CompareTo(e2.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class GenericQuery<TBase, T1, T2, T3, T4> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();
        var e4 = dataLayer.Query<T4>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e3.MoveNext()) { yield break; }
                compare = e3.Current.CompareTo(e2.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e4.MoveNext()) { yield break; }
                compare = e4.Current.CompareTo(e3.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class GenericQuery<TBase, T1, T2, T3, T4, T5> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();
        var e4 = dataLayer.Query<T4>().GetEnumerator();
        var e5 = dataLayer.Query<T5>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e3.MoveNext()) { yield break; }
                compare = e3.Current.CompareTo(e2.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e4.MoveNext()) { yield break; }
                compare = e4.Current.CompareTo(e3.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e5.MoveNext()) { yield break; }
                compare = e5.Current.CompareTo(e4.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class GenericQuery<TBase, T1, T2, T3, T4, T5, T6> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
    where T6 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();
        var e4 = dataLayer.Query<T4>().GetEnumerator();
        var e5 = dataLayer.Query<T5>().GetEnumerator();
        var e6 = dataLayer.Query<T6>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e3.MoveNext()) { yield break; }
                compare = e3.Current.CompareTo(e2.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e4.MoveNext()) { yield break; }
                compare = e4.Current.CompareTo(e3.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e5.MoveNext()) { yield break; }
                compare = e5.Current.CompareTo(e4.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e6.MoveNext()) { yield break; }
                compare = e6.Current.CompareTo(e5.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class GenericQuery<TBase, T1, T2, T3, T4, T5, T6, T7> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
    where T6 : TBase
    where T7 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();
        var e4 = dataLayer.Query<T4>().GetEnumerator();
        var e5 = dataLayer.Query<T5>().GetEnumerator();
        var e6 = dataLayer.Query<T6>().GetEnumerator();
        var e7 = dataLayer.Query<T7>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e3.MoveNext()) { yield break; }
                compare = e3.Current.CompareTo(e2.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e4.MoveNext()) { yield break; }
                compare = e4.Current.CompareTo(e3.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e5.MoveNext()) { yield break; }
                compare = e5.Current.CompareTo(e4.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e6.MoveNext()) { yield break; }
                compare = e6.Current.CompareTo(e5.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e7.MoveNext()) { yield break; }
                compare = e7.Current.CompareTo(e6.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class GenericQuery<TBase, T1, T2, T3, T4, T5, T6, T7, T8> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
    where T6 : TBase
    where T7 : TBase
    where T8 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();
        var e4 = dataLayer.Query<T4>().GetEnumerator();
        var e5 = dataLayer.Query<T5>().GetEnumerator();
        var e6 = dataLayer.Query<T6>().GetEnumerator();
        var e7 = dataLayer.Query<T7>().GetEnumerator();
        var e8 = dataLayer.Query<T8>().GetEnumerator();

        bool hit;
        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            hit = true;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e3.MoveNext()) { yield break; }
                compare = e3.Current.CompareTo(e2.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e4.MoveNext()) { yield break; }
                compare = e4.Current.CompareTo(e3.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e5.MoveNext()) { yield break; }
                compare = e5.Current.CompareTo(e4.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e6.MoveNext()) { yield break; }
                compare = e6.Current.CompareTo(e5.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e7.MoveNext()) { yield break; }
                compare = e7.Current.CompareTo(e6.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            while (true) {
                if (!e8.MoveNext()) { yield break; }
                compare = e8.Current.CompareTo(e7.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) { hit = false; }

            if (hit) {
                yield return e1.Current;
            }
        }
    }
}

public class Query<T1, T2> : GenericQuery<IComponent, T1, T2>
    where T1 : IComponent
    where T2 : IComponent
{
}

public class Query<T1, T2, T3> : GenericQuery<IComponent, T1, T2, T3>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
{
}

public class Query<T1, T2, T3, T4> : GenericQuery<IComponent, T1, T2, T3, T4>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5> : GenericQuery<IComponent, T1, T2, T3, T4, T5>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5, T6> : GenericQuery<IComponent, T1, T2, T3, T4, T5, T6>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5, T6, T7> : GenericQuery<IComponent, T1, T2, T3, T4, T5, T6, T7>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
    where T7 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5, T6, T7, T8> : GenericQuery<IComponent, T1, T2, T3, T4, T5, T6, T7, T8>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
    where T7 : IComponent
    where T8 : IComponent
{
}

internal class WithoutQuery<TBase, T1> : IQuery<TBase>
    where T1 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id));
}

internal class WithoutQuery<TBase, T1, T2> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id));
}

internal class WithoutQuery<TBase, T1, T2, T3> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id) && !dataLayer.Contains<T3>(id));
}

internal class WithoutQuery<TBase, T1, T2, T3, T4> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id) && !dataLayer.Contains<T3>(id) && !dataLayer.Contains<T4>(id));
}

internal class WithoutQuery<TBase, T1, T2, T3, T4, T5> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id) && !dataLayer.Contains<T3>(id) && !dataLayer.Contains<T4>(id)
               && !dataLayer.Contains<T5>(id));
}

internal class WithoutQuery<TBase, T1, T2, T3, T4, T5, T6> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
    where T6 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id) && !dataLayer.Contains<T3>(id) && !dataLayer.Contains<T4>(id)
               && !dataLayer.Contains<T5>(id) && !dataLayer.Contains<T6>(id));
}

internal class WithoutQuery<TBase, T1, T2, T3, T4, T5, T6, T7> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
    where T6 : TBase
    where T7 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id) && !dataLayer.Contains<T3>(id) && !dataLayer.Contains<T4>(id)
               && !dataLayer.Contains<T5>(id) && !dataLayer.Contains<T6>(id) && !dataLayer.Contains<T7>(id));
}

internal class WithoutQuery<TBase, T1, T2, T3, T4, T5, T6, T7, T8> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
    where T4 : TBase
    where T5 : TBase
    where T6 : TBase
    where T7 : TBase
    where T8 : TBase
{
    [AllowNull] public IQuery<TBase> Internal { get; init; }
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
        => Internal.Query(dataLayer).Where(
            id => !dataLayer.Contains<T1>(id) && !dataLayer.Contains<T2>(id) && !dataLayer.Contains<T3>(id) && !dataLayer.Contains<T4>(id)
               && !dataLayer.Contains<T5>(id) && !dataLayer.Contains<T6>(id) && !dataLayer.Contains<T7>(id) && !dataLayer.Contains<T8>(id));
}

public static class QueryExtensions
{
    public static IQuery<TBase> GenericWithout<TBase, T1>(this IQuery<TBase> q)
        where T1 : TBase
        => new WithoutQuery<TBase, T1> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        => new WithoutQuery<TBase, T1, T2> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2, T3>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        where T3 : TBase
        => new WithoutQuery<TBase, T1, T2, T3> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2, T3, T4>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        where T3 : TBase
        where T4 : TBase
        => new WithoutQuery<TBase, T1, T2, T3, T4> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2, T3, T4, T5>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        where T3 : TBase
        where T4 : TBase
        where T5 : TBase
        => new WithoutQuery<TBase, T1, T2, T3, T4, T5> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2, T3, T4, T5, T6>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        where T3 : TBase
        where T4 : TBase
        where T5 : TBase
        where T6 : TBase
        => new WithoutQuery<TBase, T1, T2, T3, T4, T5, T6> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2, T3, T4, T5, T6, T7>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        where T3 : TBase
        where T4 : TBase
        where T5 : TBase
        where T6 : TBase
        where T7 : TBase
        => new WithoutQuery<TBase, T1, T2, T3, T4, T5, T6, T7> { Internal = q };

    public static IQuery<TBase> GenericWithout<TBase, T1, T2, T3, T4, T5, T6, T7, T8>(this IQuery<TBase> q)
        where T1 : TBase
        where T2 : TBase
        where T3 : TBase
        where T4 : TBase
        where T5 : TBase
        where T6 : TBase
        where T7 : TBase
        where T8 : TBase
        => new WithoutQuery<TBase, T1, T2, T3, T4, T5, T6, T7, T8> { Internal = q };

    public static IQuery<IComponent> Without<T1>(this IQuery<IComponent> q)
        where T1 : IComponent
        => new WithoutQuery<IComponent, T1> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        => new WithoutQuery<IComponent, T1, T2> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2, T3>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        => new WithoutQuery<IComponent, T1, T2, T3> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2, T3, T4>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        => new WithoutQuery<IComponent, T1, T2, T3, T4> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2, T3, T4, T5>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        => new WithoutQuery<IComponent, T1, T2, T3, T4, T5> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2, T3, T4, T5, T6>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        => new WithoutQuery<IComponent, T1, T2, T3, T4, T5, T6> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2, T3, T4, T5, T6, T7>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
        => new WithoutQuery<IComponent, T1, T2, T3, T4, T5, T6, T7> { Internal = q };

    public static IQuery<IComponent> Without<T1, T2, T3, T4, T5, T6, T7, T8>(this IQuery<IComponent> q)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
        where T8 : IComponent
        => new WithoutQuery<IComponent, T1, T2, T3, T4, T5, T6, T7, T8> { Internal = q };
}
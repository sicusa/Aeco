namespace Aeco;

public interface IQuery<out TComponent>
{
    IEnumerable<Guid> Query(IDataLayer<TComponent> dataLayer);
}

public class GQuery<TBase, T1, T2> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();

        if (!(e1.MoveNext() && e2.MoveNext())) {
            yield break;
        }

        while (true) {
            var res = e1.Current.CompareTo(e2.Current);
            if (res < 0) {
                if (!e1.MoveNext()) {
                    yield break;
                }
            }
            else if (res > 0) {
                do {
                    if (!e2.MoveNext()) {
                        yield break;
                    }
                    res = e1.Current.CompareTo(e2.Current);
                } while (res > 0);
                if (res == 0) { yield return e2.Current; }
            }
            else {
                yield return e2.Current;

                if (!(e1.MoveNext() && e2.MoveNext())) {
                    yield break;
                }
            }
        }
    }
}

public class GQuery<TBase, T1, T2, T3> : IQuery<TBase>
    where T1 : TBase
    where T2 : TBase
    where T3 : TBase
{
    public IEnumerable<Guid> Query(IDataLayer<TBase> dataLayer)
    {
        var e1 = dataLayer.Query<T1>().GetEnumerator();
        var e2 = dataLayer.Query<T2>().GetEnumerator();
        var e3 = dataLayer.Query<T3>().GetEnumerator();

        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext())) {
            yield break;
        }

        while (true) {
            var res = e1.Current.CompareTo(e2.Current);
            if (res < 0) {
                if (!e1.MoveNext()) {
                    yield break;
                }
            }
            else if (res > 0) {
                do {
                    if (!e2.MoveNext()) {
                        yield break;
                    }
                    res = e1.Current.CompareTo(e2.Current);
                } while (res > 0);
                if (res == 0) { yield return e2.Current; }
            }
            else {
                res = e1.Current.CompareTo(e3.Current);
                if (res < 0) {
                    if (!(e1.MoveNext() && e2.MoveNext())) {
                        yield break;
                    }
                }
                else if (res > 0) {
                    do {
                        if (!e3.MoveNext()) {
                            yield break;
                        }
                        res = e1.Current.CompareTo(e3.Current);
                    } while (res > 0);
                    if (res == 0) { yield return e3.Current; }
                }
                else {
                    yield return e3.Current;

                    if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext())) {
                        yield break;
                    }
                }
            }
        }
    }
}

public class GQuery<TBase, T1, T2, T3, T4> : IQuery<TBase>
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

        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext())) {
            yield break;
        }

        while (true) {
            var res = e1.Current.CompareTo(e2.Current);
            if (res < 0) {
                if (!e1.MoveNext()) {
                    yield break;
                }
            }
            else if (res > 0) {
                do {
                    if (!e2.MoveNext()) {
                        yield break;
                    }
                    res = e1.Current.CompareTo(e2.Current);
                } while (res > 0);
                if (res == 0) { yield return e2.Current; }
            }
            else {
                res = e1.Current.CompareTo(e3.Current);
                if (res < 0) {
                    if (!(e1.MoveNext() && e2.MoveNext())) {
                        yield break;
                    }
                }
                else if (res > 0) {
                    do {
                        if (!e3.MoveNext()) {
                            yield break;
                        }
                        res = e1.Current.CompareTo(e3.Current);
                    } while (res > 0);
                    if (res == 0) { yield return e3.Current; }
                }
                else {
                    res = e1.Current.CompareTo(e4.Current);
                    if (res < 0) {
                        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext())) {
                            yield break;
                        }
                    }
                    else if (res > 0) {
                        do {
                            if (!e4.MoveNext()) {
                                yield break;
                            }
                            res = e1.Current.CompareTo(e4.Current);
                        } while (res > 0);
                        if (res == 0) { yield return e4.Current; }
                    }
                    else {
                        yield return e4.Current;

                        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext())) {
                            yield break;
                        }
                    }
                }
            }
        }
    }
}

public class GQuery<TBase, T1, T2, T3, T4, T5> : IQuery<TBase>
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

        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext())) {
            yield break;
        }

        while (true) {
            var res = e1.Current.CompareTo(e2.Current);
            if (res < 0) {
                if (!e1.MoveNext()) {
                    yield break;
                }
            }
            else if (res > 0) {
                do {
                    if (!e2.MoveNext()) {
                        yield break;
                    }
                    res = e1.Current.CompareTo(e2.Current);
                } while (res > 0);
                if (res == 0) { yield return e2.Current; }
            }
            else {
                res = e1.Current.CompareTo(e3.Current);
                if (res < 0) {
                    if (!(e1.MoveNext() && e2.MoveNext())) {
                        yield break;
                    }
                }
                else if (res > 0) {
                    do {
                        if (!e3.MoveNext()) {
                            yield break;
                        }
                        res = e1.Current.CompareTo(e3.Current);
                    } while (res > 0);
                    if (res == 0) { yield return e3.Current; }
                }
                else {
                    res = e1.Current.CompareTo(e4.Current);
                    if (res < 0) {
                        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext())) {
                            yield break;
                        }
                    }
                    else if (res > 0) {
                        do {
                            if (!e4.MoveNext()) {
                                yield break;
                            }
                            res = e1.Current.CompareTo(e4.Current);
                        } while (res > 0);
                        if (res == 0) { yield return e4.Current; }
                    }
                    else {
                        res = e1.Current.CompareTo(e5.Current);
                        if (res < 0) {
                            if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext())) {
                                yield break;
                            }
                        }
                        else if (res > 0) {
                            do {
                                if (!e5.MoveNext()) {
                                    yield break;
                                }
                                res = e1.Current.CompareTo(e5.Current);
                            } while (res > 0);
                            if (res == 0) { yield return e5.Current; }
                        }
                        else {
                            yield return e5.Current;

                            if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext())) {
                                yield break;
                            }
                        }
                    }
                }
            }
        }
    }
}

public class GQuery<TBase, T1, T2, T3, T4, T5, T6> : IQuery<TBase>
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

        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext())) {
            yield break;
        }

        while (true) {
            var res = e1.Current.CompareTo(e2.Current);
            if (res < 0) {
                if (!e1.MoveNext()) {
                    yield break;
                }
            }
            else if (res > 0) {
                do {
                    if (!e2.MoveNext()) {
                        yield break;
                    }
                    res = e1.Current.CompareTo(e2.Current);
                } while (res > 0);
                if (res == 0) { yield return e2.Current; }
            }
            else {
                res = e1.Current.CompareTo(e3.Current);
                if (res < 0) {
                    if (!(e1.MoveNext() && e2.MoveNext())) {
                        yield break;
                    }
                }
                else if (res > 0) {
                    do {
                        if (!e3.MoveNext()) {
                            yield break;
                        }
                        res = e1.Current.CompareTo(e3.Current);
                    } while (res > 0);
                    if (res == 0) { yield return e3.Current; }
                }
                else {
                    res = e1.Current.CompareTo(e4.Current);
                    if (res < 0) {
                        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext())) {
                            yield break;
                        }
                    }
                    else if (res > 0) {
                        do {
                            if (!e4.MoveNext()) {
                                yield break;
                            }
                            res = e1.Current.CompareTo(e4.Current);
                        } while (res > 0);
                        if (res == 0) { yield return e4.Current; }
                    }
                    else {
                        res = e1.Current.CompareTo(e5.Current);
                        if (res < 0) {
                            if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext())) {
                                yield break;
                            }
                        }
                        else if (res > 0) {
                            do {
                                if (!e5.MoveNext()) {
                                    yield break;
                                }
                                res = e1.Current.CompareTo(e5.Current);
                            } while (res > 0);
                            if (res == 0) { yield return e5.Current; }
                        }
                        else {
                            res = e1.Current.CompareTo(e6.Current);
                            if (res < 0) {
                                if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext())) {
                                    yield break;
                                }
                            }
                            else if (res > 0) {
                                do {
                                    if (!e6.MoveNext()) {
                                        yield break;
                                    }
                                    res = e1.Current.CompareTo(e6.Current);
                                } while (res > 0);
                                if (res == 0) { yield return e6.Current; }
                            }
                            else {
                                yield return e6.Current;

                                if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext())) {
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

public class GQuery<TBase, T1, T2, T3, T4, T5, T6, T7> : IQuery<TBase>
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

        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext())) {
            yield break;
        }

        while (true) {
            var res = e1.Current.CompareTo(e2.Current);
            if (res < 0) {
                if (!e1.MoveNext()) {
                    yield break;
                }
            }
            else if (res > 0) {
                do {
                    if (!e2.MoveNext()) {
                        yield break;
                    }
                    res = e1.Current.CompareTo(e2.Current);
                } while (res > 0);
                if (res == 0) { yield return e2.Current; }
            }
            else {
                res = e1.Current.CompareTo(e3.Current);
                if (res < 0) {
                    if (!(e1.MoveNext() && e2.MoveNext())) {
                        yield break;
                    }
                }
                else if (res > 0) {
                    do {
                        if (!e3.MoveNext()) {
                            yield break;
                        }
                        res = e1.Current.CompareTo(e3.Current);
                    } while (res > 0);
                    if (res == 0) { yield return e3.Current; }
                }
                else {
                    res = e1.Current.CompareTo(e4.Current);
                    if (res < 0) {
                        if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext())) {
                            yield break;
                        }
                    }
                    else if (res > 0) {
                        do {
                            if (!e4.MoveNext()) {
                                yield break;
                            }
                            res = e1.Current.CompareTo(e4.Current);
                        } while (res > 0);
                        if (res == 0) { yield return e4.Current; }
                    }
                    else {
                        res = e1.Current.CompareTo(e5.Current);
                        if (res < 0) {
                            if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext())) {
                                yield break;
                            }
                        }
                        else if (res > 0) {
                            do {
                                if (!e5.MoveNext()) {
                                    yield break;
                                }
                                res = e1.Current.CompareTo(e5.Current);
                            } while (res > 0);
                            if (res == 0) { yield return e5.Current; }
                        }
                        else {
                            res = e1.Current.CompareTo(e6.Current);
                            if (res < 0) {
                                if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext())) {
                                    yield break;
                                }
                            }
                            else if (res > 0) {
                                do {
                                    if (!e6.MoveNext()) {
                                        yield break;
                                    }
                                    res = e1.Current.CompareTo(e6.Current);
                                } while (res > 0);
                                if (res == 0) { yield return e6.Current; }
                            }
                            else {
                                res = e1.Current.CompareTo(e7.Current);
                                if (res < 0) {
                                    if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext())) {
                                        yield break;
                                    }
                                }
                                else if (res > 0) {
                                    do {
                                        if (!e7.MoveNext()) {
                                            yield break;
                                        }
                                        res = e1.Current.CompareTo(e7.Current);
                                    } while (res > 0);
                                    if (res == 0) { yield return e7.Current; }
                                }
                                else {
                                    yield return e7.Current;

                                    if (!(e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext())) {
                                        yield break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}


public class Query<T1, T2> : GQuery<IComponent, T1, T2>
    where T1 : IComponent
    where T2 : IComponent
{
}

public class Query<T1, T2, T3> : GQuery<IComponent, T1, T2, T3>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
{
}

public class Query<T1, T2, T3, T4> : GQuery<IComponent, T1, T2, T3, T4>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5> : GQuery<IComponent, T1, T2, T3, T4, T5>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5, T6> : GQuery<IComponent, T1, T2, T3, T4, T5, T6>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
{
}

public class Query<T1, T2, T3, T4, T5, T6, T7> : GQuery<IComponent, T1, T2, T3, T4, T5, T6, T7>
    where T1 : IComponent
    where T2 : IComponent
    where T3 : IComponent
    where T4 : IComponent
    where T5 : IComponent
    where T6 : IComponent
    where T7 : IComponent
{
}
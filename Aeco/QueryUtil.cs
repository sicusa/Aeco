namespace Aeco;

using System.Buffers;

public static class QueryUtil
{
    public static IEnumerable<Guid> Intersect(IEnumerable<Guid> orderedEnum1, IEnumerable<Guid> orderedEnum2)
    {
        var e1 = orderedEnum1.GetEnumerator();
        var e2 = orderedEnum2.GetEnumerator();

        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }

            if (compare != 0) {
                yield return e1.Current;
            }
        }
    }

    public static IEnumerable<Guid> Intersect(IEnumerable<IEnumerable<Guid>> orderedEnums)
    {
        var enums = orderedEnums.Select(e => e.GetEnumerator()).ToArray();
        var count = enums.Length;
        if (count == 0) { yield break; }

        var headEnum = enums[0];
        bool hit;
        int compare;

        while (true) {
            if (!headEnum.MoveNext()) { yield break; }
            hit = true;

            for (int i = 1; i < count; ++i) {
                var prevE = enums[i - 1];
                var e = enums[i];

                while (true) {
                    if (!e.MoveNext()) { yield break; }
                    compare = e.Current.CompareTo(prevE.Current);
                    if (compare >= 0) { break; }
                }
                if (compare != 0) { hit = false; }
            }

            if (hit) {
                yield return headEnum.Current;
            }
        }
    }

    public static IEnumerable<Guid> Union(IEnumerable<Guid> orderedEnum1, IEnumerable<Guid> orderedEnum2)
    {
        var e1 = orderedEnum1.GetEnumerator();
        var e2 = orderedEnum2.GetEnumerator();

        int compare;

        while (true) {
            if (!e1.MoveNext()) { yield break; }
            yield return e1.Current;

            while (true) {
                if (!e2.MoveNext()) { yield break; }
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
            }
            if (compare != 0) {
                yield return e1.Current;
            }
        }
    }

    public static IEnumerable<Guid> Union(IEnumerable<IEnumerable<Guid>> orderedEnums)
    {
        var enums = orderedEnums.Select(e => e.GetEnumerator()).ToArray();
        var count = enums.Length;
        if (count == 0) { yield break; }

        var headEnum = enums[0];
        int compare;

        while (true) {
            if (!headEnum.MoveNext()) { yield break; }
            yield return headEnum.Current;

            for (int i = 1; i < count; ++i) {
                var prevE = enums[i - 1];
                var e = enums[i];

                while (true) {
                    if (!e.MoveNext()) { yield break; }
                    compare = e.Current.CompareTo(prevE.Current);
                    if (compare >= 0) { break; }
                }

                if (compare != 0) {
                    yield return e.Current;
                }
            }
        }
    }
}
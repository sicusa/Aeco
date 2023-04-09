namespace Aeco;

using System.Buffers;

public static class QueryUtil
{
    public static IEnumerable<uint> Intersect(IEnumerable<uint> orderedEnum1, IEnumerable<uint> orderedEnum2)
    {
        var e1 = orderedEnum1.GetEnumerator();
        var e2 = orderedEnum2.GetEnumerator();

        if (!(e1.MoveNext() && e2.MoveNext())) {
            yield break;
        }

        int compare;

        while (true) {
            while (true) {
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
                if (!e2.MoveNext()) { yield break; }
            }

            if (compare == 0) {
                yield return e1.Current;
            }
            if (!e1.MoveNext()) { yield break; }
        }
    }

    public static IEnumerable<uint> Intersect(IEnumerable<IEnumerable<uint>> orderedEnums)
    {
        var enums = orderedEnums.Select(e => e.GetEnumerator()).ToArray();
        var count = enums.Length;
        if (count == 0) { yield break; }

        for (int i = 0; i != enums.Length; ++i) {
            if (!enums[i].MoveNext()) {
                yield break;
            }
        }

        var headEnum = enums[0];
        bool hit;
        int compare;

        while (true) {
            hit = true;

            for (int i = 1; i < count; ++i) {
                var prevE = enums[i - 1];
                var e = enums[i];

                while (true) {
                    compare = e.Current.CompareTo(prevE.Current);
                    if (compare >= 0) { break; }
                    if (!e.MoveNext()) { yield break; }
                }
                if (compare != 0) { hit = false; }
            }

            if (hit) {
                yield return headEnum.Current;
            }
            if (!headEnum.MoveNext()) { yield break; }
        }
    }

    public static IEnumerable<uint> Union(IEnumerable<uint> orderedEnum1, IEnumerable<uint> orderedEnum2)
    {
        var e1 = orderedEnum1.GetEnumerator();
        var e2 = orderedEnum2.GetEnumerator();

        if (!(e1.MoveNext() && e2.MoveNext())) {
            yield break;
        }

        int compare;

        while (true) {
            yield return e1.Current;

            while (true) {
                compare = e2.Current.CompareTo(e1.Current);
                if (compare >= 0) { break; }
                if (!e2.MoveNext()) { yield break; }
            }
            if (compare != 0) {
                yield return e1.Current;
            }
            if (!e1.MoveNext()) { yield break; }
        }
    }

    public static IEnumerable<uint> Union(IEnumerable<IEnumerable<uint>> orderedEnums)
    {
        var enums = orderedEnums.Select(e => e.GetEnumerator()).ToArray();
        var count = enums.Length;
        if (count == 0) { yield break; }

        for (int i = 0; i != enums.Length; ++i) {
            if (!enums[i].MoveNext()) {
                yield break;
            }
        }

        var headEnum = enums[0];
        int compare;

        while (true) {
            yield return headEnum.Current;

            for (int i = 1; i < count; ++i) {
                var prevE = enums[i - 1];
                var e = enums[i];

                while (true) {
                    compare = e.Current.CompareTo(prevE.Current);
                    if (compare >= 0) { break; }
                    if (!e.MoveNext()) { yield break; }
                }

                if (compare != 0) {
                    yield return e.Current;
                }
            }
            if (!headEnum.MoveNext()) { yield break; }
        }
    }
}
namespace Aeco;

using System.Buffers;

public static class EntityUtil
{
    public static IEnumerable<Guid> Intersect(IEnumerable<IEnumerable<Guid>> orderedEnums, int count)
        => count == 1 ? orderedEnums.First() : RawIntersect(orderedEnums, count);

    private static IEnumerable<Guid> RawIntersect(IEnumerable<IEnumerable<Guid>> orderedEnums, int count)
    {
        var pool = ArrayPool<IEnumerator<Guid>>.Shared;
        var enums = pool.Rent(count);

        try {
            int i = 0;
            foreach (var orderedEnum in orderedEnums) {
                if (i >= count) { break; }
                enums[i] = orderedEnum.GetEnumerator();
                ++i;
            }

            var headEnum = enums[0];
            bool hit;
            int compare;

            while (true) {
                if (!headEnum.MoveNext()) { yield break; }
                hit = true;

                for (i = 1; i < count; ++i) {
                    var prevE = enums[i - 1];
                    var e = enums[i];

                    while (true) {
                        if (!e.MoveNext()) { yield break; }
                        compare = e.Current.CompareTo(prevE);
                        if (compare >= 0) { break; }
                    }
                    if (compare != 0) { hit = false; }
                }

                if (hit) {
                    yield return headEnum.Current;
                }
            }
        }
        finally {
            pool.Return(enums);
        }
    }
}
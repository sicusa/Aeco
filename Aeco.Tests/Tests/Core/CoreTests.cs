namespace Aeco.Tests;

using Aeco;

public static class CoreTests
{
    public static void Run()
    {
        Console.WriteLine("== Core ==");

        TestQueryUtil();
    }

    public static void TestQueryUtil()
    {
        Guid[] CreateGuids(int count) =>
            Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();

        var commonIds = CreateGuids(10);
        var ids1 = CreateGuids(10).Concat(commonIds).OrderBy(id => id);
        var ids2 = CreateGuids(10).Concat(commonIds).OrderBy(id => id);

        Console.WriteLine("Intersect: " + QueryUtil.Intersect(ids1, ids2).Count());
        Console.WriteLine("Union: " + QueryUtil.Union(ids1, ids2).Count());
    }
}
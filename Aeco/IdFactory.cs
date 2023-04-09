namespace Aeco;

public static class IdFactory
{
    private static uint _acc = 0;

    public static uint New() => Interlocked.Increment(ref _acc);
}
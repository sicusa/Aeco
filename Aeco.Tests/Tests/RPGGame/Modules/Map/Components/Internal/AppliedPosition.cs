namespace Aeco.Tests.RPGGame.Map;

public struct AppliedPosition : IPooledGameComponent
{
    public int X;
    public int Y;

    public void Set(in Position pos)
    {
        X = pos.X;
        Y = pos.Y;
    }

    public static implicit operator (int X, int Y)(in AppliedPosition pos)
        => (pos.X, pos.Y);
}
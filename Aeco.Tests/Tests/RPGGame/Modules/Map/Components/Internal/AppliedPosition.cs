namespace Aeco.Tests.RPGGame.Map;

using System.Runtime.Serialization;

[DataContract]
public struct AppliedPosition : IPooledGameComponent
{
    [DataMember] public int X;
    [DataMember] public int Y;

    public void Set(in Position pos)
    {
        X = pos.X;
        Y = pos.Y;
    }

    public void Dispose()
    {
        X = 0;
        Y = 0;
    }

    public static implicit operator (int X, int Y)(in AppliedPosition pos)
        => (pos.X, pos.Y);
}
namespace Aeco.Tests.RPGGame.Map;

using System.Runtime.Serialization;

[DataContract]
public struct Position : IPooledGameComponent
{
    [DataMember] public int X;
    [DataMember] public int Y;

    public void Dispose()
    {
        X = 0;
        Y = 0;
    }

    public static implicit operator (int X, int Y)(in Position pos)
        => (pos.X, pos.Y);
}
namespace Aeco.Tests.RPGGame.Map;

using System.Runtime.Serialization;

[DataContract]
public struct Position : IPooledGameComponent
{
    [DataMember] public int X;
    [DataMember] public int Y;

    public static implicit operator (int X, int Y)(in Position pos)
        => (pos.X, pos.Y);
}
namespace Aeco.Tests.RPGGame.Map;

using System.Runtime.Serialization;

[DataContract]
public struct Rotation : IPooledGameComponent
{
    [DataMember] public Direction Value = Direction.Up;

    public Rotation() {}

    public void Dispose()
    {
        Value = Direction.Up;
    }
}
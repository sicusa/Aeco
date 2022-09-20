namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Move : ICommand
{
    [DataMember]
    public Direction Direction;

    public void Dispose()
    {
        Direction = Direction.Up;
    }
}
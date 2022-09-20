namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Turn : ICommand
{
    [DataMember]
    public Guid ActorId;
    [DataMember]
    public Direction Direction;

    public void Dispose()
    {
        ActorId = Guid.Empty;
        Direction = Direction.Up;
    }
}
namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct PickUp : ICommand
{
    [DataMember]
    public Guid ActorId;

    public void Dispose()
    {
        ActorId = Guid.Empty;
    }
}
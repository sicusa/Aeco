namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Location : IComponent
{
    [DataMember]
    public Guid MapId;
}
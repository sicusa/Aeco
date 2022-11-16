namespace Aeco.Tests.RPGGame.Map;

using System.Runtime.Serialization;

[DataContract]
public struct InMap : IPooledGameComponent
{
    [DataMember] public Guid MapId;
}
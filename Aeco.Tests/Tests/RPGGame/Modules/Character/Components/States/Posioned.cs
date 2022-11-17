namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

[DataContract]
public struct Posioned : IGameComponent
{
    [DataMember] public float Duration;
    [DataMember] public float DamageRate;
}
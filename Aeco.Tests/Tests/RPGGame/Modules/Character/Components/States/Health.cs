namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

[DataContract]
public struct Health : IGameComponent
{
    [DataMember] public float Value = 100;

    public Health() {}
}
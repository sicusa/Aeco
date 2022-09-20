namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Health : IComponent
{
    [DataMember]
    public float Value = 100;
    public Health() {}
}
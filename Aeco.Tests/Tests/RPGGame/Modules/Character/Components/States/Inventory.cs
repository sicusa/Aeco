namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

[DataContract]
public struct Inventory : IGameComponent
{
    [DataMember] public Dictionary<Type, Guid> Items = new();

    public Inventory() {}
}
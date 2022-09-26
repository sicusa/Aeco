namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

[DataContract]
public struct Equipments : IGameComponent
{
    [DataMember] public IWeapon? Weapon;
}
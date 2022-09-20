namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Equipments : IComponent
{
    [DataMember]
    public IWeapon? Weapon;
}
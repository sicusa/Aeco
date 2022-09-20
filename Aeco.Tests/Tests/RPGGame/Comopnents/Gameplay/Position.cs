namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Position : IComponent
{
    [DataMember]
    public int X;
    [DataMember]
    public int Y;
}
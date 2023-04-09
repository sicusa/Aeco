namespace Aeco.Tests.RPGGame.Gameplay;

using System.Runtime.Serialization;

[DataContract]
public struct SavedGameIndex : IGameComponent
{
    [DataMember]
    public uint[] SavedGames;
}
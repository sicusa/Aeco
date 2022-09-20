namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct SavedGameIndex : ISavedComponent
{
    [DataMember]
    public Guid[] SavedGames;
}
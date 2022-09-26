namespace Aeco.Tests.RPGGame.Gameplay;

using System.Runtime.Serialization;

[DataContract]
public struct SavedGame : IGameComponent
{
    public DateTime ModifyTime;
}
namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct SavedGame : ISavedComponent
{
    public DateTime ModifyTime;
}
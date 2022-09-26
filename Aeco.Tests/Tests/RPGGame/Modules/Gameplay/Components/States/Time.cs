namespace Aeco.Tests.RPGGame.Gameplay;

using System.Runtime.Serialization;

[DataContract]
public class Time : IPooledGameComponent
{
    [DataMember] public float Value;

    public void Dispose()
    {
        Value = 0;
    }
}
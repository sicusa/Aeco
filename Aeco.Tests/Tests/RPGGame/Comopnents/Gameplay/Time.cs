namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public class Time : IComponent, IDisposable
{
    [DataMember]
    public float Value;

    public void Dispose()
    {
        Value = 0;
    }
}
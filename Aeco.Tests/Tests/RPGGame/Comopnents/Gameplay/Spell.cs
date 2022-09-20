namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public class Spell : IComponent, IDisposable
{
    public void Dispose()
    {
    }
}
namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Destroy : ICommand
{
    public void Dispose()
    {
    }
}
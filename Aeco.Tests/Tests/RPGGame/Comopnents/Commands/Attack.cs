namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Attack : ICommand
{
    public void Dispose()
    {
    }
}
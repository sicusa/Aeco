namespace Aeco.Tests.RPGGame;

using System.Runtime.Serialization;

[DataContract]
public struct Map : IComponent
{
    public Dictionary<(int, int), HashSet<Guid>> Blocks = new();
    public Map() {}
}
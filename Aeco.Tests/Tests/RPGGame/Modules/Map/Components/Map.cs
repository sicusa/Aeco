namespace Aeco.Tests.RPGGame.Map;

using System.Runtime.Serialization;

[DataContract]
public struct Map : IGameComponent
{
    [DataMember] public Dictionary<(int, int), HashSet<Guid>> Blocks = new();

    public Map() {}
    
    public bool AddObject(in (int, int) pos, Guid id)
    {
        if (!Blocks.TryGetValue(pos, out var objectIds)) {
            objectIds = new HashSet<Guid>();
            Blocks.Add(pos, objectIds);
        }
        return objectIds.Add(id);
    }

    public bool RemoveObject(in (int, int) pos, Guid id)
    {
        if (!Blocks.TryGetValue(pos, out var objectIds)
                || !objectIds.Remove(id)) {
            return false;
        }
        if (objectIds.Count == 0) {
            Blocks.Remove(pos);
        }
        return true;
    }
}
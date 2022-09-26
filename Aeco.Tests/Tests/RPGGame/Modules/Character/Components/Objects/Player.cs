namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

using Aeco.Tests.RPGGame.Map;

[DataContract]
public struct Player : IPooledGameComponent
{
    public void Dispose()
    {
    }
}

public static class PlayerExtensions
{
    public static IEntity<IGameComponent> AsPlayer(this IEntity<IGameComponent> entity, Guid mapId)
    {
        entity.Acquire<Player>();
        entity.Acquire<InMap>().MapId = mapId;
        entity.Acquire<Health>();
        entity.Acquire<Attackable>();
        entity.Acquire<Equipments>();
        return entity;
    }
}
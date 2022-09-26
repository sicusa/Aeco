namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

using Aeco.Tests.RPGGame.Map;

[DataContract]
public struct Enemy : IPooledGameComponent
{
    public void Dispose()
    {
    }
}

public static class EnemyExtensions
{
    public static IEntity<IComponent> AsEnemy(this IEntity<IComponent> entity, Guid mapId)
    {
        entity.Acquire<Enemy>();
        entity.Acquire<InMap>().MapId = mapId;
        entity.Acquire<Health>();
        entity.Acquire<Attackable>();
        entity.Acquire<Equipments>();
        return entity;
    }
}
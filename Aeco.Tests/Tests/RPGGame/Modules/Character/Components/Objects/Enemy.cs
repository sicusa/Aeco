namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

using Aeco.Tests.RPGGame.Map;

[DataContract]
public struct Enemy : IPooledGameComponent
{
}

public static class EnemyExtensions
{
    public static void MakeEnemy(this IExpandableDataLayer<IComponent> dataLayer, Guid id, Guid mapId)
    {
        dataLayer.Acquire<Enemy>(id);
        dataLayer.Acquire<InMap>(id).MapId = mapId;
        dataLayer.Acquire<Health>(id);
        dataLayer.Acquire<Attackable>(id);
        dataLayer.Acquire<Equipments>(id);
    }
}
namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

using Aeco.Tests.RPGGame.Map;

[DataContract]
public struct Player : IPooledGameComponent
{
}

public static class PlayerExtensions
{
    public static void MakePlayer(this IExpandableDataLayer<IComponent> dataLayer, uint id, uint mapId)
    {
        dataLayer.Acquire<Player>(id);
        dataLayer.Acquire<InMap>(id).MapId = mapId;
        dataLayer.Acquire<Health>(id);
        dataLayer.Acquire<Attackable>(id);
        dataLayer.Acquire<Equipments>(id);
    }
}
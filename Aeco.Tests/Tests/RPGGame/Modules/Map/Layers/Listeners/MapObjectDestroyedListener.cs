namespace Aeco.Tests.RPGGame.Map;

using Aeco.Tests.RPGGame.Gameplay;

public class MapObjectDestroyedListener : Layer, IGameLateUpdateLayer
{
    private Query<InMap, Position, Destroy> _q = new();

    public void LateUpdate(RPGGame game)
    {
        foreach (var id in _q.Query(game)) {
            ref readonly var pos = ref game.Inspect<Position>(id);
            ref var map = ref game.Require<Map>(game.Inspect<InMap>(id).MapId);
            map.RemoveObject(pos, id);
        }
    }
}
namespace Aeco.Tests.RPGGame.Map;

using Aeco.Reactive;

public class PositionHandler : Layer, IGameUpdateLayer
{
    private Query<Modified<Position>, Position, InMap> _q = new();

    public void Update(RPGGame game)
    {
        foreach (var id in _q.Query(game)) {
            ref readonly var pos = ref game.Inspect<Position>(id);
            ref var appliedPos = ref game.Acquire<AppliedPosition>(id);
            ref var map = ref game.Require<Map>(game.Inspect<InMap>(id).MapId);
            map.RemoveObject(appliedPos, id);
            map.AddObject(pos, id);
            appliedPos.Set(pos);
        }
    }
}
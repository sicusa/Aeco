namespace Aeco.Tests.RPGGame.Map;

using Aeco.Reactive;

public class InMapHandler : VirtualLayer, IGameUpdateLayer
{
    private Query<Created<InMap>, InMap> _q = new();

    public void Update(RPGGame game)
    {
        foreach (var id in _q.Query(game)) {
            ref var map = ref game.Require<Map>(game.Inspect<InMap>(id).MapId);
            ref var pos = ref game.Acquire<Position>(id);
            game.Acquire<AppliedPosition>(id).Set(pos);
            game.Acquire<Rotation>(id);
            map.AddObject(pos, id);
        }
    }
}
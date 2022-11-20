namespace Aeco.Tests.RPGGame.Character;

using Aeco.Tests.RPGGame.Map;

public class MoveHandler : VirtualLayer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Position>()) {
            ref var pos = ref game.Require<Position>(id);
            while (game.Remove(id, out Move move)) {
                ref readonly var cmd = ref game.Inspect<Move>(id);
                switch (cmd.Direction) {
                case Direction.Up:
                    ++pos.Y;
                    break;
                case Direction.Down:
                    --pos.Y;
                    break;
                case Direction.Right:
                    ++pos.X;
                    break;
                case Direction.Left:
                    --pos.X;
                    break;
                }
            }
        }
    }
}
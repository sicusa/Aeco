namespace Aeco.Tests.RPGGame;

public class MoveInterpreter : VirtualLayer, IGameLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Move>()) {
            ref var cmd = ref game.Require<Move>(id);
            ref var pos = ref game.Require<Position>(id);

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

            game.Remove<Move>(id);
        }
    }
}
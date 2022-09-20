namespace Aeco.Tests.RPGGame;

public class TimeUpdator : VirtualLayer, IGameLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Time>()) {
            game.Acquire<Time>(id).Value += game.DeltaTime;
        }
    }
}
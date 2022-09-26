namespace Aeco.Tests.RPGGame.Gameplay;

public class TimeUpdator : VirtualLayer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Time>()) {
            game.Acquire<Time>(id).Value += game.DeltaTime;
        }
    }
}
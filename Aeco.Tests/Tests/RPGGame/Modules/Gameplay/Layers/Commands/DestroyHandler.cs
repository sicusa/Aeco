namespace Aeco.Tests.RPGGame.Gameplay;

public class DestroyHandler : VirtualLayer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        foreach (var entityId in game.Query<Destroy>()) {
            game.Clear(entityId);
        }
    }
}
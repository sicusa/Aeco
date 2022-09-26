namespace Aeco.Tests.RPGGame.Gameplay;

public class DestroyHandler : VirtualLayer, IGameLateUpdateLayer
{
    public void LateUpdate(RPGGame game)
    {
        foreach (var entityId in game.Query<Destroy>()) {
            game.Clear(entityId);
        }
    }
}
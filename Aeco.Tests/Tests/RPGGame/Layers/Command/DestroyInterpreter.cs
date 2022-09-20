namespace Aeco.Tests.RPGGame;

public class DestroyInterpreter : VirtualLayer, IGameLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Destroy>()) {
            game.Clear(id);
        }
    }
}
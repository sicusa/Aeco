namespace Aeco.Tests.RPGGame.Character;

using Aeco.Tests.RPGGame.Gameplay;

public class DeadCharacterHandler : VirtualLayer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Dead>()) {
            Console.WriteLine($"Dead character: {id}");
            game.Acquire<Destroy>(id);
        }
    }
}
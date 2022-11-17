namespace Aeco.Tests.RPGGame.Character;

public class HealthUpdator : VirtualLayer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        var dt = game.DeltaTime;
        foreach (var id in game.Query<Health>()) {
            ref var health = ref game.Require<Health>(id);
            if (game.Contains<Posioned>(id)) {
                ref var posioned = ref game.Require<Posioned>(id);
                health.Value -= posioned.DamageRate * dt;
                posioned.Duration -= dt;
                if (posioned.Duration <= 0) {
                    game.Remove<Posioned>(id);
                }
            }
            if (health.Value <= 0) {
                game.Acquire<Dead>(id);
            }
        }
    }
}
namespace Aeco.Tests.RPGGame;

public class AttackInterpreter : VirtualLayer, IGameLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Attack>()) {
            ref var cmd = ref game.Require<Attack>(id);
            ref var weapon = ref game.Require<Equipments>(id).Weapon;
            if (weapon != null) {
                ref var map = ref game.Require<Map>(game.Require<Location>(id).MapId);
                foreach (var pos in weapon.GetEffectiveArea(game, id)) {
                    if (!map.Blocks.TryGetValue(pos, out var targets)) {
                        continue;
                    }
                    foreach (var targetId in targets) {
                        if (game.Contains<Attackable>(targetId)) {
                            weapon.Attack(game, id, targetId);
                        }
                    }
                }
            }
            game.Remove<Attack>(id);
        }
    }
}
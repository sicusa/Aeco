namespace Aeco.Tests.RPGGame.Character;

using Aeco.Tests.RPGGame.Map;

public class AttackHandler : Layer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Equipments>()) {
            var weapon = game.Require<Equipments>(id).Weapon;
            if (weapon == null) {
                continue;
            }
            while (game.Remove(id, out Attack cmd)) {
                ref readonly var map = ref game.Inspect<Map>(game.Inspect<InMap>(id).MapId);
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
        }
    }
}
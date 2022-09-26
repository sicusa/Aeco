namespace Aeco.Tests.RPGGame.Character;

using Aeco.Tests.RPGGame.Map;

public class AttackHandler : VirtualLayer, IGameUpdateLayer
{
    public void Update(RPGGame game)
    {
        foreach (var id in game.Query<Attack>()) {
            try {
                ref readonly var cmd = ref game.Inspect<Attack>(id);
                var weapon = game.Inspect<Equipments>(id).Weapon;
                if (weapon == null) {
                    Console.WriteLine($"[{id}] Failed to execute Attack command: Weapon not found.");
                    continue;
                }
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
            finally {
                game.Remove<Attack>(id);
            }
        }
    }
}
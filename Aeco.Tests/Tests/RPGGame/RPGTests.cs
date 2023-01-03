namespace Aeco.Tests;

using Aeco.Persistence;

using Aeco.Tests.RPGGame;
using Aeco.Tests.RPGGame.Character;
using Aeco.Tests.RPGGame.Gameplay;
using Aeco.Tests.RPGGame.Map;

public static class RPGTests
{
    public static void Run()
    {
        var game = new RPGGame.RPGGame(new Config {});
        game.Load();

        // frame 0

        game.Update(0.5f);

        var map = game.GetEntity<Map>();
        var mapBlocks = game.Require<Map>(map.Id).Blocks;

        var player = game.GetEntity<Player>();
        player.Acquire<Equipments>().Weapon = new LongSword();

        ref var pos = ref player.Require<Position>();
        ref var rot = ref player.Require<Rotation>();
        Console.WriteLine("Found in map: " + mapBlocks[pos].Contains(player.Id)); // true

        ++pos.X;
        rot.Value = Direction.Left;

        // frame 1

        game.Update(0.5f);
        Console.WriteLine("Found in map: " + mapBlocks[pos].Contains(player.Id)); // false

        var enemy = game.CreateEntity().AsEnemy(map.Id);

        var ids = new List<Guid>();
        for (int i = 0; i < 2000; ++i) {
            var entity = game.CreateEntity();
            entity.AsEnemy(map.Id);
            ids.Add(entity.Id);
        }
        foreach (var id in ids) {
            if (!game.Contains<Enemy>(id)) {
                Console.WriteLine("Not found");
            }
        }
        Console.WriteLine("Enemy count: " + game.GetCount<Enemy>());

        // frame 2

        game.Update(0.5f);
        player.Set(new Attack());

        // frame 3

        game.Update(0.5f);
        Console.WriteLine("Enemy HP: " + enemy.Require<Health>().Value);

        player.Acquire<Equipments>().Weapon = new PoisonousLongSword();
        player.Set(new Attack());

        // frame 4

        game.Update(0.5f);
        Console.WriteLine("Enemy HP: " + enemy.Require<Health>().Value);
        Console.WriteLine("Enemy posioned: " + enemy.Contains<Posioned>());

        // frame 5 ~ 6

        for (int i = 5; i <= 6; ++i) {
            game.Update(0.5f);
            Console.WriteLine("Enemy HP: " + enemy.Require<Health>().Value);
            Console.WriteLine("Enemy posioned: " + enemy.Contains<Posioned>());
        }

        player.Set(new Attack());

        // frame 7

        game.Update(0.5f);

        Console.WriteLine("Enemy Alive: " + enemy.Contains<Health>());
        Console.WriteLine("Enemy count: " + game.GetCount<Enemy>());

        player.Acquire<Persistent>();
        player.Dispose();

        enemy.Acquire<Persistent>();
        enemy.Dispose();

        map.Acquire<Persistent>();
        map.Dispose();
    }
}
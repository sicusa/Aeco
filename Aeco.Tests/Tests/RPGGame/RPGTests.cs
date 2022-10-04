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

        // frame 2

        game.Update(0.5f);
        player.Acquire<Attack>();

        // frame 3

        game.Update(0.5f);
        Console.WriteLine("Enemy HP: " + enemy.Require<Health>().Value);

        player.Acquire<Equipments>().Weapon = new PoisonousLongSword();
        player.Acquire<Attack>();

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

        player.Acquire<Attack>();

        // frame 7

        game.Update(0.5f);
        Console.WriteLine("Enemy Alive: " + enemy.Contains<Health>());

        player.Acquire<Persistent>();
        player.Dispose();

        enemy.Acquire<Persistent>();
        enemy.Dispose();

        map.Acquire<Persistent>();
        map.Dispose();
    }
}
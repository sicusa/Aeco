namespace Aeco.Tests;

using System.Linq;

using Aeco.Tests.RPGGame;
using Aeco.Tests.RPGGame.Character;
using Aeco.Tests.RPGGame.Map;

public static class RPGTests
{
    public static void Run()
    {
        var game = new RPGGame.RPGGame(new Config {});
        game.Load();

        // frame 0

        game.Update(0.5f);

        var mapId = game.Singleton<Map>()!.Value;
        var mapBlocks = game.Require<Map>(mapId).Blocks;

        var playerId = game.Singleton<Player>()!.Value;
        game.Acquire<Equipments>(playerId).Weapon = new LongSword();

        ref var pos = ref game.Require<Position>(playerId);
        ref var rot = ref game.Require<Rotation>(playerId);
        Console.WriteLine("Found in map: " + mapBlocks[pos].Contains(playerId)); // true

        ++pos.X;
        rot.Value = Direction.Left;

        // frame 1

        game.Update(0.5f);
        Console.WriteLine("Found in map: " + mapBlocks[pos].Contains(playerId)); // false

        var enemyId = IdFactory.New();
        game.MakeEnemy(enemyId, mapId);

        var compRef = game.GetRef<Health>(enemyId);
        Console.WriteLine(compRef.GetRef().Value);

        var ids = new List<uint>();
        for (int i = 0; i < 2000; ++i) {
            var otherEnemyId = IdFactory.New();
            game.MakeEnemy(otherEnemyId, mapId);
            ids.Add(otherEnemyId);
        }

        var removedIds = ids.Take(1000).ToArray();
        foreach (var id in removedIds) {
            game.Clear(id);
        }
        Console.WriteLine("Enemy count: " + game.GetCount<Enemy>());

        foreach (var id in removedIds) {
            game.MakeEnemy(id, mapId);
        }
        Console.WriteLine("Enemy count: " + game.GetCount<Enemy>());

        // frame 2

        game.Update(0.5f);
        game.Set(playerId, new Attack());


        // frame 3

        game.Update(0.5f);
        Console.WriteLine("Enemy HP: " + game.Require<Health>(enemyId).Value);

        game.Acquire<Equipments>(playerId).Weapon = new PoisonousLongSword();
        game.Set(playerId, new Attack());

        // frame 4

        game.Update(0.5f);
        Console.WriteLine("Enemy HP: " + game.Require<Health>(enemyId).Value);
        Console.WriteLine("Enemy posioned: " + game.Contains<Posioned>(enemyId));

        // frame 5 ~ 6

        for (int i = 5; i <= 6; ++i) {
            game.Update(0.5f);
            Console.WriteLine("Enemy HP: " + game.Require<Health>(enemyId).Value);
            Console.WriteLine("Enemy posioned: " + game.Contains<Posioned>(enemyId));
        }

        game.Set(playerId, new Attack());

        // frame 7

        game.Update(0.5f);

        Console.WriteLine("Enemy Alive: " + game.Contains<Health>(enemyId));
        Console.WriteLine("Enemy count: " + game.GetCount<Enemy>());
    }
}
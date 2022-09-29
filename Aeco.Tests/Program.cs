using System.Numerics;
using System.Reflection;

using Aeco;
using Aeco.Renderer.GL;
using Aeco.Reactive;

using Aeco.Tests;
using Aeco.Tests.RPGGame.Character;
using Aeco.Persistence;

// LocalTests.Run();
// ReactiveTests.Run();

var game = new Aeco.Tests.RPGGame.RPGGame(new Aeco.Tests.RPGGame.Config());

game.Initialize(new RendererSpec {
    Width = 800,
    Height = 600,
    Title = "RPG Game"
});

var cameraId = Guid.NewGuid();
game.Acquire<Camera>(cameraId);
game.Acquire<Position>(cameraId).Value = new Vector3(0, 0, 4f);

var textureId = Guid.NewGuid();
ref var texture = ref game.Acquire<Texture>(textureId);
texture.Stream = InternalAssets.Load("Textures.wall.jpg");

var materialId = Guid.NewGuid();
ref var material = ref game.Acquire<Material>(materialId);
material.ShaderProgram = GLRendererCompositeLayer.DefaultShaderProgramId;
material.Texture = textureId;

var firstId = Guid.Empty;
var prevId = Guid.Empty;
for (int i = 0; i < 50; ++i) {
    var renderableId = Guid.NewGuid();
    ref var renderable = ref game.Acquire<MeshRenderable>(renderableId);
    renderable.Mesh = Polygons.Cube;
    renderable.Materials = new Guid[] { materialId };
    game.Acquire<Position>(renderableId).Value = new Vector3(i * 0.1f - 0.1f, 0, 0);
    if (prevId != Guid.Empty) {
        game.Acquire<Parent>(renderableId).Id = prevId;
    }
    else {
        firstId = renderableId;
        game.Acquire<Scale>(renderableId).Value = new Vector3(0.2f);
    }
    prevId = renderableId;
}

float x = 0;
float y = 0;
var window = game.Require<Window>().Current!;

float Lerp(float firstFloat, float secondFloat, float by)
    => firstFloat * (1 - by) + secondFloat * by;

window.UpdateFrame += e => {
    float rate = game.DeltaTime * 10;
    x = Lerp(x, -window.MousePosition.X * 0.005f, rate);
    y = Lerp(y, window.MousePosition.Y * 0.005f, rate);

    //game.Acquire<Position>(cameraId).Value = new Vector3(0, 0, game.Time);
    game.Acquire<Rotation>(cameraId).Value = Quaternion.CreateFromYawPitchRoll(0, 0, game.Time);

    foreach (var id in game.Query<MeshRenderable>()) {
        game.Acquire<Rotation>(id).Value = Quaternion.CreateFromYawPitchRoll(x, y, 0);
        // game.Acquire<Position>(id).Value = new Vector3(x, y, 0);
    }
};

game.Run();

/*
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
*/
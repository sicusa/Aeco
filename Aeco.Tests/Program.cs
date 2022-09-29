using System.Numerics;

using OpenTK.Windowing.GraphicsLibraryFramework;

using Aeco.Renderer.GL;

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

Guid CreateCube(in Vector3 pos, Guid? parent = null)
{
    var renderableId = Guid.NewGuid();
    ref var renderable = ref game!.Acquire<MeshRenderable>(renderableId);
    renderable.Mesh = Polygons.Cube;
    renderable.Materials = new Guid[] { materialId };
    if (parent != null) {
        game.Acquire<Parent>(renderableId).Id = parent.Value;
    }
    game.Acquire<Position>(renderableId).Value = pos;
    return renderableId;
}

Guid prevId = CreateCube(Vector3.Zero);
game.Acquire<Scale>(prevId).Value = new Vector3(0.2f);

for (int i = 0; i < 50; ++i) {
    prevId = CreateCube(new Vector3(i * 0.1f - 0.1f, 0, 0), prevId);
    game.Acquire<Scale>(prevId).Value = new Vector3(0.95f);
}

float Lerp(float firstFloat, float secondFloat, float by)
    => firstFloat * (1 - by) + secondFloat * by;

var window = game.Require<Aeco.Renderer.GL.Window>().Current!;

float rate = 10;
float sensitivity = 0.005f;
float x = 0;
float y = 0;

window.UpdateFrame += e => {
    float scaledRate = game.DeltaTime * rate;
    x = Lerp(x, window.MousePosition.X * sensitivity, scaledRate);
    y = Lerp(y, window.MousePosition.Y * sensitivity, scaledRate);

    var rot = Quaternion.CreateFromYawPitchRoll(x, -y, 0);
    game.Acquire<Rotation>(cameraId).Value = rot;

    float time = game.Time;
    foreach (var id in game.Query<MeshRenderable>()) {
        game.Acquire<Rotation>(id).Value = Quaternion.CreateFromYawPitchRoll(time, time, time);
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
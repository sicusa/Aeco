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
game.Acquire<Parent>(cameraId).Id = GLRenderer.RootId;

var textureId = Guid.NewGuid();
ref var texture = ref game.Acquire<Texture>(textureId);
texture.Stream = InternalAssets.Load("Textures.wall.jpg");

var materialId = Guid.NewGuid();
ref var material = ref game.Acquire<Material>(materialId);
material.ShaderProgram = GLRenderer.DefaultShaderProgramId;
material.Texture = textureId;

Guid CreateCube(in Vector3 pos, Guid parentId)
{
    var renderableId = Guid.NewGuid();
    ref var renderable = ref game!.Acquire<MeshRenderable>(renderableId);
    renderable.Mesh = Polygons.Cube;
    renderable.Materials = new Guid[] { materialId };
    game.Acquire<Parent>(renderableId).Id = parentId;
    game.Acquire<Position>(renderableId).Value = pos;
    return renderableId;
}

Guid prevId = CreateCube(Vector3.Zero, GLRenderer.RootId);
game.Acquire<Scale>(prevId).Value = new Vector3(0.3f);

for (int i = 0; i < 50; ++i) {
    prevId = CreateCube(new Vector3(i * 0.1f - 0.1f, 0, 0), prevId);
    game.Acquire<Scale>(prevId).Value = new Vector3(0.95f);
}

Guid rotatorId = CreateCube(Vector3.Zero, GLRenderer.RootId);
game.Acquire<Scale>(rotatorId).Value = new Vector3(0.3f);

float Lerp(float firstFloat, float secondFloat, float by)
    => firstFloat * (1 - by) + secondFloat * by;

var window = game.RequireAny<Aeco.Renderer.GL.Window>().Current!;

float rate = 10;
float sensitivity = 0.005f;
float x = 0;
float y = 0;

window.RenderFrame += e => {
    float scaledRate = game.DeltaTime * rate;
    x = Lerp(x, window.MousePosition.X * sensitivity, scaledRate);
    y = Lerp(y, window.MousePosition.Y * sensitivity, scaledRate);

    float time = game.Time;
    var rot = Quaternion.CreateFromYawPitchRoll(x, -y, 0);
    foreach (var id in game.Query<MeshRenderable>()) {
        if (id == rotatorId) { continue; }
        game.Acquire<Rotation>(id).Value = rot;
    }

    ref var rotatorView = ref game.Acquire<WorldView>(rotatorId);
    game.Acquire<Position>(rotatorId).Value += rotatorView.Forward * game.DeltaTime * 2;
    game.Acquire<Rotation>(rotatorId).Value *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, game.DeltaTime);

    //var rot = Quaternion.CreateFromYawPitchRoll(x, -y, 0);
    //game.Acquire<Rotation>(cameraId).Value = rot;
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
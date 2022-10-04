namespace Aeco.Tests;

using System.Numerics;

using OpenTK.Windowing.GraphicsLibraryFramework;

using Aeco.Local;
using Aeco.Reactive;
using Aeco.Renderer.GL;

public static class GLTests
{
    public static void Run()
    {
        var debugLayer = new DebugLayer();
        var eventDataLayer = new PolyPoolStorage<IReactiveEvent>();
        var game = new GLRenderer(
            eventDataLayer: eventDataLayer,
            new AutoClearCompositeLayer(eventDataLayer),
            debugLayer);

        game.Initialize(new RendererSpec {
            Width = 800,
            Height = 600,
            Title = "RPG Game"
        });

        var cameraId = Guid.Parse("c2003019-0b2a-4f4c-ba31-9930c958ff83");
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
        Guid firstId = prevId;
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

        float time = 0f;
        float rate = 10;
        float sensitivity = 0.005f;
        float x = 0;
        float y = 0;
        Vector3 currDeltaPos = Vector3.Zero;
        bool moving = false;

        debugLayer.OnUpdate += deltaTime => {
            float scaledRate = deltaTime * rate;

            time += deltaTime;
            x = Lerp(x, (window.MousePosition.X - window.Size.X / 2) * sensitivity, scaledRate);
            y = Lerp(y, (window.MousePosition.Y - window.Size.Y / 2) * sensitivity, scaledRate);

            foreach (var id in game.Query<MeshRenderable>()) {
                if (id == rotatorId) { continue; }
                game.Acquire<Rotation>(id).Value = Quaternion.CreateFromYawPitchRoll(time, time, time);
            }

            ref readonly var rotatorView = ref game.Inspect<WorldView>(rotatorId);
            ref var rotatorPos = ref game.Acquire<Position>(rotatorId).Value;
            rotatorPos += rotatorView.Forward * deltaTime * 2;
            game.Acquire<Rotation>(rotatorId).Value *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, deltaTime);

            game.Acquire<WorldView>(firstId).Forward = rotatorPos;
            game.Acquire<Rotation>(cameraId).Value = Quaternion.CreateFromYawPitchRoll(-x, -y, 0);

            var deltaPos = Vector3.Zero;
            bool modified = false;
            if (window.KeyboardState.IsKeyDown(Keys.W)) {
                deltaPos += game.Inspect<WorldView>(cameraId).Forward;
                moving = true;
                modified = true;
            }
            if (window.KeyboardState.IsKeyDown(Keys.S)) {
                deltaPos -= game.Inspect<WorldView>(cameraId).Forward;
                moving = true;
                modified = true;
            }
            if (window.KeyboardState.IsKeyDown(Keys.A)) {
                deltaPos -= game.Inspect<WorldView>(cameraId).Right;
                moving = true;
                modified = true;
            }
            if (window.KeyboardState.IsKeyDown(Keys.D)) {
                deltaPos += game.Inspect<WorldView>(cameraId).Right;
                moving = true;
                modified = true;
            }
            if (moving) {
                currDeltaPos = Vector3.Lerp(currDeltaPos, deltaPos, scaledRate);
                if (!modified && currDeltaPos.Length() < 0.001f) {
                    moving = false;
                    currDeltaPos = Vector3.Zero;
                }
                else {
                    ref var pos = ref game.Acquire<Position>(cameraId).Value;
                    pos += currDeltaPos * deltaTime * 5;
                }
            }
        };

        game.Run();
    }
}
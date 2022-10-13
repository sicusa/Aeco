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

        var mainLight = game.CreateEntity();
        mainLight.Acquire<Parent>().Id = GLRenderer.RootId;
        mainLight.Acquire<Rotation>().Value = Quaternion.CreateFromYawPitchRoll(-90, -45, 0);
        mainLight.Acquire<MainLight>().Color = new Vector4(1, 1, 1, 0.5f);

        var cameraId = Guid.Parse("c2003019-0b2a-4f4c-ba31-9930c958ff83");
        game.Acquire<Camera>(cameraId);
        game.Acquire<Position>(cameraId).Value = new Vector3(0, 0, 4f);
        game.Acquire<Parent>(cameraId).Id = GLRenderer.RootId;

        var model = InternalAssets.Load<ModelResource>("Models.sphere.glb");
        var cubeMesh = model.RootNode!.Meshes![0];
        cubeMesh.Material = new MaterialResource {
            AmbientColor = new Vector4(0.2f),
            DiffuseColor = new Vector4(1, 1, 1, 1),
            Shininess = 0.5f
        };
        cubeMesh.Material.Textures[TextureType.Diffuse] =
            new TextureResource(InternalAssets.Load<ImageResource>("Textures.wall.jpg"));

        Guid CreateCube(in Vector3 pos, Guid parentId)
        {
            var id = Guid.NewGuid();
            game.Acquire<Renderable>(id);
            game.Acquire<Mesh>(id).Resource = cubeMesh;
            game.Acquire<Parent>(id).Id = parentId;
            game.Acquire<Position>(id).Value = pos;
            return id;
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

            foreach (var id in game.Query<Renderable>()) {
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
namespace Aeco.Tests;

using System.Numerics;

using OpenTK.Windowing.GraphicsLibraryFramework;

using Aeco.Local;
using Aeco.Reactive;
using Aeco.Renderer.GL;

public static class GLTests
{
    public struct Rotator : IGLObject
    {
        public void Dispose() => this = new();
    }

    public static void Run()
    {
        var debugLayer = new DebugLayer();
        var eventDataLayer = new PolyPoolStorage<IReactiveEvent>();
        var game = new GLRenderer(
            eventDataLayer: eventDataLayer,
            new AutoClearCompositeLayer(eventDataLayer),
            debugLayer);

        game.IsProfileEnabled = true;
        game.Initialize(new RendererSpec {
            Width = 1920 / 2,
            Height = 1080 /2,
            UpdateFrequency = 60,
            RenderFrequency = 60,
            //IsFullscreen = true,
            Title = "RPG Game"
            //IsDebugEnabled = true
        });

        void PrintLayerProfiles<TLayer>(string name, IEnumerable<(TLayer, GLRenderer.LayerProfile)> profiles)
        {
            Console.WriteLine($"[{name} Layer Profiles]");
            foreach (var (layer, profile) in profiles.OrderByDescending(v => v.Item2.AverangeTime)) {
                Console.WriteLine($"  {layer}: avg={profile.AverangeTime}, max={profile.MaximumTime}, min={profile.MinimumTime}");
            }
        }

        debugLayer.OnUnload += () => {
            Console.WriteLine();
            PrintLayerProfiles("Update", game.UpdateLayerProfiles);
            PrintLayerProfiles("LateUpdate", game.LateUpdateLayerProfiles);
            PrintLayerProfiles("Render", game.RenderLayerProfiles);
        };

        debugLayer.OnLateLoad += () => Launch(game, debugLayer);
        game.Run();
    }

    private static void Launch(GLRenderer game, DebugLayer debugLayer)
    {
        var sunLight = game.CreateEntity();
        sunLight.Acquire<Position>().Value = new Vector3(0, 1, 5);
        sunLight.Acquire<Rotation>().Value = Quaternion.CreateFromYawPitchRoll(-90, -45, 0);
        sunLight.Acquire<Light>().Resource = new DirectionalLightResource {
            Color = new Vector4(1, 1, 1, 0.1f)
        };

        var spotLight = game.CreateEntity();
        spotLight.Acquire<Position>().Value = new Vector3(0, 1, 0);
        spotLight.Acquire<Light>().Resource = new SpotLightResource {
            Color = new Vector4(0.5f, 1, 0.5f, 5),
            InnerConeAngle = 25,
            OuterConeAngle = 40,
            AttenuationQuadratic = 1
        };

        var pointLight = game.CreateEntity();
        pointLight.Acquire<Position>().Value = new Vector3(0, 1, 0);
        pointLight.Acquire<Light>().Resource = new PointLightResource {
            Color = new Vector4(1, 1, 1, 1),
            AttenuationQuadratic = 0.7f
        };

        var pointLight2 = game.CreateEntity();
        pointLight2.Acquire<Position>().Value = new Vector3(0, 1, 0);
        pointLight2.Acquire<Light>().Resource = new PointLightResource {
            Color = new Vector4(1, 0.5f, 1, 2),
            AttenuationQuadratic = 3f
        };

        var cameraId = Guid.Parse("c2003019-0b2a-4f4c-ba31-9930c958ff83");
        game.Acquire<Camera>(cameraId);
        game.Acquire<Position>(cameraId).Value = new Vector3(0, 0, 4f);
        game.Acquire<Parent>(cameraId).Id = GLRenderer.RootId;

        var wallTex = new TextureResource(InternalAssets.Load<ImageResource>("Textures.wall.jpg"));

        var torusModel = InternalAssets.Load<ModelResource>("Models.torus.glb");
        var torusMesh = torusModel.RootNode!.Meshes![0];
        torusMesh.Material = new MaterialResource {
            Parameters = new() {
                AmbientColor = new Vector4(1),
                DiffuseColor = new Vector4(1, 1, 1, 1),
                SpecularColor = new Vector4(0.3f),
                Shininess = 32
            }
        };
        torusMesh.Material.Textures[TextureType.Diffuse] = wallTex;

        var torusMeshTransparent = torusModel.RootNode!.Meshes![0] with {
            Material = new MaterialResource {
                IsTransparent = true,
                Parameters = new() {
                    AmbientColor = new Vector4(1),
                    DiffuseColor = new Vector4(1, 1, 1, 0.3f),
                    SpecularColor = new Vector4(0.3f),
                    Shininess = 32
                }
            }
        };
        torusMeshTransparent.Material.Textures[TextureType.Diffuse] = wallTex;

        var sphereModel = InternalAssets.Load<ModelResource>("Models.sphere.glb");
        var sphereMesh = sphereModel.RootNode!.Meshes![0];
        sphereMesh.Material = new MaterialResource {
            Parameters = new() {
                AmbientColor = new Vector4(0.2f),
                DiffuseColor = new Vector4(1, 1, 1, 1),
                SpecularColor = new Vector4(0.3f),
                EmissiveColor = new Vector4(0.8f, 1f, 0.8f, 2f),
                Shininess = 32
            }
        };
        sphereMesh.Material.Textures[TextureType.Diffuse] =
            new TextureResource(InternalAssets.Load<ImageResource>("Textures.wall.jpg"));

        Guid CreateObject(in Vector3 pos, Guid parentId, MeshResource mesh)
        {
            var id = Guid.NewGuid();
            ref var renderable = ref game.Acquire<MeshRenderable>(id);
            renderable.Mesh = mesh;
            game.Acquire<Parent>(id).Id = parentId;
            game.Acquire<Position>(id).Value = pos;
            return id;
        }

        Guid CreateLight(in Vector3 pos, Guid parentId)
        {
            var id = Guid.NewGuid();
            game.Acquire<Light>(id).Resource = new PointLightResource {
                Color = new Vector4(1, 1, 1, 5),
                AttenuationQuadratic = 25f
            };
            game.Acquire<Parent>(id).Id = parentId;
            game.Acquire<Position>(id).Value = pos;
            return id;
        }

        Guid prevId = CreateObject(Vector3.Zero, GLRenderer.RootId, sphereMesh);
        pointLight2.Acquire<Parent>().Id = prevId;

        Guid firstId = prevId;
        game.Acquire<Scale>(prevId).Value = new Vector3(0.3f);

        for (int i = 0; i < 5000; ++i) {
            prevId = CreateObject(new Vector3(MathF.Sin(i) * i * 0.1f, 0, MathF.Cos(i) * i * 0.1f), firstId,
                i % 2 == 0 ? torusMesh : torusMeshTransparent);
            game.Acquire<Scale>(prevId).Value = new Vector3(0.99f);
        }

        for (int i = 50; i < 250; i += 2) {
            var id = CreateLight(new Vector3(MathF.Sin(i) * i * 0.1f, 0, MathF.Cos(i) * i * 0.1f), GLRenderer.RootId);
            game.Acquire<Rotator>(id);
        }

        Guid rotatorId = CreateObject(Vector3.Zero, GLRenderer.RootId, sphereMesh);
        game.Acquire<Scale>(rotatorId).Value = new Vector3(0.3f);
        game.Acquire<Rotator>(rotatorId);
        spotLight.Acquire<Parent>().Id = rotatorId;
        pointLight.Acquire<Parent>().Id = rotatorId;

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

        ref RenderTargetDebug GetDebug()
            => ref game.Acquire<RenderTargetDebug>(GLRenderer.DefaultRenderTargetId);

        debugLayer.OnUpdate += deltaTime => {
            float scaledRate = deltaTime * rate;

            time += deltaTime;
            x = Lerp(x, (window.MousePosition.X - window.Size.X / 2) * sensitivity, scaledRate);
            y = Lerp(y, (window.MousePosition.Y - window.Size.Y / 2) * sensitivity, scaledRate);

            foreach (var rotatorId in game.Query<Rotator>()) {
                ref readonly var rotatorAxes = ref game.Inspect<WorldAxes>(rotatorId);
                ref var rotatorPos = ref game.Acquire<Position>(rotatorId).Value;
                rotatorPos += rotatorAxes.Forward * deltaTime * 2;
                game.Acquire<Rotation>(rotatorId).Value = Quaternion.CreateFromAxisAngle(Vector3.UnitY, time);
            }

            if (window.KeyboardState.IsKeyPressed(Keys.F1)) {
                game.RemoveAny<RenderTargetDebug>();
            }
            if (window.KeyboardState.IsKeyPressed(Keys.F2)) {
                GetDebug().DisplayMode = DisplayMode.TransparencyAccum;
            }
            if (window.KeyboardState.IsKeyPressed(Keys.F3)) {
                GetDebug().DisplayMode = DisplayMode.TransparencyAlpha;
            }
            if (window.KeyboardState.IsKeyPressed(Keys.F4)) {
                GetDebug().DisplayMode = DisplayMode.Depth;
            }
            if (window.KeyboardState.IsKeyPressed(Keys.F5)) {
                GetDebug().DisplayMode = DisplayMode.Clusters;
            }

            game.Acquire<Rotation>(cameraId).Value = Quaternion.CreateFromYawPitchRoll(-x, -y, 0);

            var deltaPos = Vector3.Zero;
            bool modified = false;
            if (window.KeyboardState.IsKeyDown(Keys.W)) {
                deltaPos += game.Inspect<WorldAxes>(cameraId).Forward;
                moving = true;
                modified = true;
            }
            if (window.KeyboardState.IsKeyDown(Keys.S)) {
                deltaPos -= game.Inspect<WorldAxes>(cameraId).Forward;
                moving = true;
                modified = true;
            }
            if (window.KeyboardState.IsKeyDown(Keys.A)) {
                deltaPos -= game.Inspect<WorldAxes>(cameraId).Right;
                moving = true;
                modified = true;
            }
            if (window.KeyboardState.IsKeyDown(Keys.D)) {
                deltaPos += game.Inspect<WorldAxes>(cameraId).Right;
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
    }
}
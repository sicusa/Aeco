namespace Aeco.Tests.RPGGame;

using Aeco.Local;
using Aeco.Reactive;
using Aeco.Serialization.Json;
using Aeco.Persistence.Local;

using Aeco.Tests.RPGGame.Character;

public class RPGGame : CompositeLayer
{
    public float Time { get; private set; }
    public float DeltaTime { get; private set; }

    private IGameUpdateLayer[] _updateLayers;
    private IGameLateUpdateLayer[] _lateUpdateLayers;

    public RPGGame(Config config)
    {
        var eventDataLayer = new PolyHashStorage<IReactiveEvent>();

        InternalAddSublayers(
            new Character.Layers(eventDataLayer),
            new Map.Layers(eventDataLayer),
            new Gameplay.Layers(),

            eventDataLayer,

            new ChannelLayer<IGameCommand>(),
            new PolyPoolStorage<IPooledGameComponent>(),
            new PolyHashStorage<IGameComponent>(),
            new PolyHashStorage(),

            new FileSystemPersistenceLayer<IGameComponent>(
                "./Save", new JsonEntitySerializer<IGameComponent>()),
            //new ReadOnlyFileSystemPersistenceLayer(
            //    "./Data", new JsonEntitySerializer<IComponent>())

            new ShortLivedCompositeLayer(eventDataLayer)
        );
        
        EntityFactory = new EntityFactory<IComponent>();
        _updateLayers = GetSublayersRecursively<IGameUpdateLayer>().ToArray();
        _lateUpdateLayers = GetSublayersRecursively<IGameLateUpdateLayer>().ToArray();
    }

    public void Load()
    {
        Console.WriteLine("RPGGame loading...");

        foreach (var layer in GetSublayersRecursively<IGameInitializeLayer>()) {
            layer.Initialize(this);
        }

        // Initialize map
        var mapId = Guid.NewGuid();
        Acquire<Map.Map>(mapId);

        // Initialize player
        this.CreateEntity().AsPlayer(mapId);
    }

    public void Update(float deltaTime)
    {
        DeltaTime = deltaTime;
        Time += DeltaTime;

        for (int i = 0; i < _updateLayers.Length; ++i) {
            _updateLayers[i].Update(this);
        }
        for (int i = 0; i < _lateUpdateLayers.Length; ++i) {
            _lateUpdateLayers[i].LateUpdate(this);
        }
    }
}
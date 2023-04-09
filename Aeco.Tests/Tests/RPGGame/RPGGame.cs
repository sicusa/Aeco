namespace Aeco.Tests.RPGGame;

using Aeco.Local;
using Aeco.Reactive;

using Aeco.Tests.RPGGame.Character;

public class RPGGame : CompositeLayer
{
    public float Time { get; private set; }
    public float DeltaTime { get; private set; }

    private IGameUpdateLayer[] _updateLayers;
    private IGameLateUpdateLayer[] _lateUpdateLayers;

    public RPGGame(Config config)
    {
        var eventStorage = new PolyHashStorage<IReactiveEvent>();
        var anyEventStorage = new PolyHashStorage<IAnyReactiveEvent>();

        InternalAddSublayers(
            new Character.Layers(),
            new Map.Layers(eventStorage, anyEventStorage),
            new Gameplay.Layers(),

            eventStorage,
            anyEventStorage,

            new ChannelLayer<IGameCommand>(),
            new PolyDenseStorage<IPooledGameComponent>(),
            new PolyHashStorage<IGameComponent>(),
            new PolyHashStorage<IComponent>(),

            new ShortLivedCompositeLayer(eventStorage)
        );
        
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
        var mapId = IdFactory.New();
        Acquire<Map.Map>(mapId);

        // Initialize player
        this.MakePlayer(IdFactory.New(), mapId);
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
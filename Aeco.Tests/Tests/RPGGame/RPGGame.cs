namespace Aeco.Tests.RPGGame;

using Aeco.Local;
using Aeco.Serialization;
using Aeco.Serialization.Json;
using Aeco.Persistence;
using Aeco.Persistence.Local;

public class RPGGame : CompositeLayer
{
    public float Time { get; private set; }
    public float DeltaTime { get; private set; }

    private IGameLayer[] _gameLayers;

    public RPGGame(Config config)
        : base(
            new PolyPoolStorage<ICommand>(),
            new MonoPoolStorage<Spell>(),
            new PolyHashStorage(),

            new FileSystemPersistenceLayer<ISavedComponent>(
                new PolyHashStorage(),
                "./Save", new JsonEntitySerializer<ISavedComponent>()),
            new ReadOnlyFileSystemPersistenceLayer(
                new PolyHashStorage(),
                "./Data", new JsonEntitySerializer<IComponent>()),

            new AttackInterpreter(),
            new DestroyInterpreter())
    {
        EntityFactory = new EntityFactory();
        _gameLayers = GetSublayers<IGameLayer>().ToArray();
    }

    public void Update(float deltaTime)
    {
        DeltaTime = deltaTime;
        Time += DeltaTime;

        for (int i = 0; i < _gameLayers.Length; ++i) {
            _gameLayers[i].Update(this);
        }
    }
}
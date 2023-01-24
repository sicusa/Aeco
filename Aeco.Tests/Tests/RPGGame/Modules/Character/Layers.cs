namespace Aeco.Tests.RPGGame.Character;

using Aeco.Local;

public class Layers : CompositeLayer
{
    public Layers()
        : base(
            // command handlers
            new AttackHandler(),
            new MoveHandler(),

            // state updaters
            new HealthUpdator(),

            // listeners
            new CharacterDeadListener()
        )
    {
    }
}
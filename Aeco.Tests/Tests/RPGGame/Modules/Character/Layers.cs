namespace Aeco.Tests.RPGGame.Character;

using Aeco.Local;
using Aeco.Reactive;

public class Layers : CompositeLayer
{
    public Layers(IExpandableDataLayer<IReactiveEvent> eventDataLayer)
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
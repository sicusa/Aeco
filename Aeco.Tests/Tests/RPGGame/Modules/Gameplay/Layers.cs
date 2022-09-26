namespace Aeco.Tests.RPGGame.Gameplay;

using Aeco.Local;

public class Layers : CompositeLayer
{
    public Layers()
        : base(
            // state updaters
            new TimeUpdator(),

            // command handlers
            new DestroyHandler()
        )
    {
    }
}
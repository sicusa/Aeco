namespace Aeco.Tests.RPGGame.Map;

using Aeco.Local;
using Aeco.Reactive;

public class Layers : CompositeLayer
{
    public Layers(IDataLayer<IReactiveEvent> eventDataLayer)
        : base(
            // storages
            new ReactiveCompositeLayer(
                eventDataLayer: eventDataLayer,
                new MonoHashStorage<InMap>(),
                new MonoHashStorage<Position>(),
                new MonoHashStorage<Rotation>()
            ),

            // reactive handlers
            new InMapHandler(),
            new PositionHandler(),

            // listeners
            new MapObjectDestroyedListener()
        )
    {
    }
}
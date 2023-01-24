namespace Aeco.Tests.RPGGame.Map;

using Aeco.Local;
using Aeco.Reactive;

public class Layers : CompositeLayer
{
    public Layers(IExpandableDataLayer<IReactiveEvent> eventDataLayer, IExpandableDataLayer<IAnyReactiveEvent> anyEventDataLayer)
        : base(
            // storages
            new ReactiveCompositeLayer(
                new MonoHashStorage<InMap>(),
                new MonoHashStorage<Position>(),
                new MonoHashStorage<Rotation>()) {
                EventDataLayer = eventDataLayer,
                AnyEventDataLayer = anyEventDataLayer
            },

            // reactive handlers
            new InMapHandler(),
            new PositionHandler(),

            // listeners
            new MapObjectDestroyedListener()
        )
    {
    }
}
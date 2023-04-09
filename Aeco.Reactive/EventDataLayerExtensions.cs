namespace Aeco.Reactive;

public static class EventDataLayerExtensions
{
    public static void MarkModified<TComponent>(this IExpandableDataLayer<IComponent> dataLayer, uint id)
        where TComponent : IComponent
    {
        dataLayer.Acquire<AnyModified<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<Modified<TComponent>>(id);
    }

    public static void MarkAnyModified<TComponent>(this IExpandableDataLayer<IComponent> dataLayer)
        where TComponent : IComponent
    {
        dataLayer.Acquire<AnyModified<TComponent>>(ReactiveCompositeLayer.AnyEventId);
    }

    public static void MarkCreated<TComponent>(this IExpandableDataLayer<IComponent> dataLayer, uint id)
        where TComponent : IComponent
    {
        dataLayer.Acquire<AnyCreated<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<AnyCreatedOrRemoved<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<Created<TComponent>>(id);
    }

    public static void MarkAnyCreated<TComponent>(this IExpandableDataLayer<IComponent> dataLayer)
        where TComponent : IComponent
    {
        dataLayer.Acquire<AnyCreated<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<AnyCreatedOrRemoved<TComponent>>(ReactiveCompositeLayer.AnyEventId);
    }
    
    public static void MarkRemoved<TComponent>(this IExpandableDataLayer<IComponent> dataLayer, uint id)
        where TComponent : IComponent
    {
        dataLayer.Acquire<AnyRemoved<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<AnyCreatedOrRemoved<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<Removed<TComponent>>(id);
    }

    public static void MarkAnyRemoved<TComponent>(this IExpandableDataLayer<IComponent> dataLayer)
        where TComponent : IComponent
    {
        dataLayer.Acquire<AnyRemoved<TComponent>>(ReactiveCompositeLayer.AnyEventId);
        dataLayer.Acquire<AnyCreatedOrRemoved<TComponent>>(ReactiveCompositeLayer.AnyEventId);
    }
}
namespace Aeco.Reactive;

using System;
using System.Diagnostics.CodeAnalysis;

using Aeco.Local;

public class ReactiveCompositeLayer<TComponent, TSublayer> : CompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public override bool IsSublayerCachable => false;

    public IDataLayer<IReactiveEvent> EventDataLayer { get; }

    private bool _existsTemp;
    
    public ReactiveCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params TSublayer[] sublayers)
        : base(sublayers)
    {
        EventDataLayer = eventDataLayer;
    }

    public override ref UComponent UnsafeInspect<UComponent>(Guid entityId)
        => ref base.Require<UComponent>(entityId);

    public override ref readonly UComponent Inspect<UComponent>(Guid entityId)
        => ref base.Require<UComponent>(entityId);

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        ref UComponent comp = ref base.Require<UComponent>(entityId);
        EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(entityId, out _existsTemp);
        if (!_existsTemp) {
            EventDataLayer.Acquire<Created<UComponent>>(entityId);
            EventDataLayer.Acquire<AnyCreated<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(entityId, out exists);
        if (!exists) {
            EventDataLayer.Acquire<Created<UComponent>>(entityId);
            EventDataLayer.Acquire<AnyCreated<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent UnsafeAcquire<UComponent>(Guid entityId)
        => ref base.Acquire<UComponent>(entityId);

    public override ref UComponent UnsafeAcquire<UComponent>(Guid entityId, out bool exists)
        => ref base.Acquire<UComponent>(entityId, out exists);
    
    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
    {
        ref UComponent comp = ref base.Set(entityId, component);
        EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        if (base.Remove<UComponent>(entityId)) {
            EventDataLayer.Acquire<Removed<UComponent>>(entityId);
            EventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            return true;
        }
        return false;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (base.Remove<UComponent>(entityId, out component)) {
            EventDataLayer.Acquire<Removed<UComponent>>(entityId);
            EventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            return true;
        }
        return false;
    }

    public override void RemoveAll<UComponent>()
    {
        foreach (var id in Query<UComponent>()) {
            EventDataLayer.Acquire<Removed<UComponent>>(id);
            EventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        base.RemoveAll<UComponent>();
    }

    public override void Clear(Guid entityId)
    {
        foreach (var comp in GetAll(entityId)) {
            var compType = comp.GetType();
            LayerUtil<IReactiveEvent>.DynamicAcquire(
                EventDataLayer, typeof(Removed<>).MakeGenericType(compType), entityId);
            LayerUtil<IReactiveEvent>.DynamicAcquire(
                EventDataLayer, typeof(AnyRemoved<>).MakeGenericType(compType), ReactiveCompositeLayer.AnyEventId);
            LayerUtil<IReactiveEvent>.DynamicAcquire(
                EventDataLayer, typeof(AnyCreatedOrRemoved<>).MakeGenericType(compType), ReactiveCompositeLayer.AnyEventId);
        }
        base.Clear(entityId);
    }

    public override void Clear()
    {
        foreach (var entityId in Query()) {
            foreach (var comp in GetAll(entityId)) {
                var compType = comp.GetType();
                LayerUtil<IReactiveEvent>.DynamicAcquire(
                    EventDataLayer, typeof(Removed<>).MakeGenericType(compType), entityId);
                LayerUtil<IReactiveEvent>.DynamicAcquire(
                    EventDataLayer, typeof(AnyRemoved<>).MakeGenericType(compType), ReactiveCompositeLayer.AnyEventId);
                LayerUtil<IReactiveEvent>.DynamicAcquire(
                    EventDataLayer, typeof(AnyCreatedOrRemoved<>).MakeGenericType(compType), ReactiveCompositeLayer.AnyEventId);
            }
        }
        base.Clear();
    }
}

public class ReactiveCompositeLayer<TComponent> : ReactiveCompositeLayer<TComponent, ILayer<TComponent>>
{
    public ReactiveCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<TComponent>[] sublayers)
        : base(eventDataLayer, sublayers)
    {
    }
}

public class ReactiveCompositeLayer : ReactiveCompositeLayer<IComponent>
{
    public static readonly Guid AnyEventId = Guid.NewGuid();

    public ReactiveCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<IComponent>[] sublayers)
        : base(eventDataLayer, sublayers)
    {
    }
}
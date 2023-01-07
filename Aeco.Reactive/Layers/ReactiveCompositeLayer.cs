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

    public override ref UComponent InspectRaw<UComponent>(Guid id)
        => ref base.Require<UComponent>(id);

    public override ref readonly UComponent Inspect<UComponent>(Guid id)
        => ref base.Require<UComponent>(id);

    public override ref UComponent Require<UComponent>(Guid id)
    {
        ref UComponent comp = ref base.Require<UComponent>(id);
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid id)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(id, out _existsTemp);
        if (!_existsTemp) {
            EventDataLayer.Acquire<Created<UComponent>>(id);
            EventDataLayer.Acquire<AnyCreated<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(id, out exists);
        if (!exists) {
            EventDataLayer.Acquire<Created<UComponent>>(id);
            EventDataLayer.Acquire<AnyCreated<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent AcquireRaw<UComponent>(Guid id)
        => ref base.Acquire<UComponent>(id);

    public override ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        => ref base.Acquire<UComponent>(id, out exists);
    
    public override ref UComponent Set<UComponent>(Guid id, in UComponent component)
    {
        ref UComponent comp = ref base.Set(id, component);
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        EventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override bool Remove<UComponent>(Guid id)
    {
        if (base.Remove<UComponent>(id)) {
            EventDataLayer.Acquire<Removed<UComponent>>(id);
            EventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            EventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            return true;
        }
        return false;
    }

    public override bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        if (base.Remove<UComponent>(id, out component)) {
            EventDataLayer.Acquire<Removed<UComponent>>(id);
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
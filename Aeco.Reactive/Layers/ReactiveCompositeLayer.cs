namespace Aeco.Reactive;

using System;
using System.Diagnostics.CodeAnalysis;

using Aeco.Local;

public class ReactiveCompositeLayer<TComponent, TSublayer> : CompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public override bool IsProvidedLayerCachable => false;

    public required IExpandableDataLayer<IReactiveEvent> EventDataLayer { get; init; }
    public required IExpandableDataLayer<IAnyReactiveEvent> AnyEventDataLayer { get; init; }
    
    public ReactiveCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public override ref UComponent InspectRaw<UComponent>(uint id)
        => ref base.Require<UComponent>(id);

    public override ref readonly UComponent Inspect<UComponent>(uint id)
        => ref base.Require<UComponent>(id);

    public override ref UComponent Require<UComponent>(uint id)
    {
        ref UComponent comp = ref base.Require<UComponent>(id);
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        AnyEventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(uint id)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(id, out bool exists);
        if (!exists) {
            EventDataLayer.Acquire<Created<UComponent>>(id);
            AnyEventDataLayer.Acquire<AnyCreated<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            AnyEventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        AnyEventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(uint id, out bool exists)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(id, out exists);
        if (!exists) {
            EventDataLayer.Acquire<Created<UComponent>>(id);
            AnyEventDataLayer.Acquire<AnyCreated<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            AnyEventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        AnyEventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override ref UComponent AcquireRaw<UComponent>(uint id)
        => ref base.Acquire<UComponent>(id);

    public override ref UComponent AcquireRaw<UComponent>(uint id, out bool exists)
        => ref base.Acquire<UComponent>(id, out exists);
    
    public override ref UComponent Set<UComponent>(uint id, in UComponent component)
    {
        ref UComponent comp = ref base.Set(id, component);
        EventDataLayer.Acquire<Modified<UComponent>>(id);
        AnyEventDataLayer.Acquire<AnyModified<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        return ref comp;
    }

    public override bool Remove<UComponent>(uint id)
    {
        if (base.Remove<UComponent>(id)) {
            EventDataLayer.Acquire<Removed<UComponent>>(id);
            AnyEventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            AnyEventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            return true;
        }
        return false;
    }

    public override bool Remove<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
    {
        if (base.Remove<UComponent>(id, out component)) {
            EventDataLayer.Acquire<Removed<UComponent>>(id);
            AnyEventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            AnyEventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            return true;
        }
        return false;
    }

    public override void RemoveAll<UComponent>()
    {
        foreach (var id in Query<UComponent>()) {
            EventDataLayer.Acquire<Removed<UComponent>>(id);
            AnyEventDataLayer.Acquire<AnyRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
            AnyEventDataLayer.Acquire<AnyCreatedOrRemoved<UComponent>>(ReactiveCompositeLayer.AnyEventId);
        }
        base.RemoveAll<UComponent>();
    }

    public override void Clear()
    {
        base.Clear();
        AnyEventDataLayer.Acquire<AnyCleared>(ReactiveCompositeLayer.AnyEventId);
    }

    public override void Clear(uint id)
    {
        base.Clear(id);
        AnyEventDataLayer.Acquire<AnyCleared>(ReactiveCompositeLayer.AnyEventId);
    }
}

public class ReactiveCompositeLayer<TComponent> : ReactiveCompositeLayer<TComponent, ILayer<TComponent>>
{
    public ReactiveCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class ReactiveCompositeLayer : ReactiveCompositeLayer<IComponent>
{
    public static readonly uint AnyEventId = IdFactory.New();

    public ReactiveCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
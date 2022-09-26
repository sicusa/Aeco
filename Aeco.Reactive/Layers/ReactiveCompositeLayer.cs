namespace Aeco.Reactive;

using System;
using System.Diagnostics.CodeAnalysis;

using Aeco.Local;

public class ReactiveCompositeLayer<TComponent, TSublayer> : CompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public override bool IsTerminalDataLayer => true;

    public IDataLayer<IReactiveEvent> EventDataLayer { get; }

    private bool _existsTemp;
    
    public ReactiveCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params TSublayer[] sublayers)
        : base(sublayers)
    {
        EventDataLayer = eventDataLayer;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(entityId, out _existsTemp);
        if (_existsTemp) {
            EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        }
        else {
            EventDataLayer.Acquire<Created<UComponent>>(entityId);
        }
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(entityId, out exists);
        if (exists) {
            EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        }
        else {
            EventDataLayer.Acquire<Created<UComponent>>(entityId);
        }
        return ref comp;
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        ref UComponent comp = ref base.Require<UComponent>(entityId);
        EventDataLayer.Acquire<Modified<UComponent>>(entityId);
        return ref comp;
    }

    public override void Set<UComponent>(Guid entityId, in UComponent component)
    {
        base.Set(entityId, component);
        EventDataLayer.Acquire<Modified<UComponent>>(entityId);
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        if (base.Remove<UComponent>(entityId)) {
            EventDataLayer.Acquire<Removed<UComponent>>(entityId);
            return true;
        }
        return false;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (base.Remove<UComponent>(entityId, out component)) {
            EventDataLayer.Acquire<Removed<UComponent>>(entityId);
            return true;
        }
        return false;
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
    public ReactiveCompositeLayer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<IComponent>[] sublayers)
        : base(eventDataLayer, sublayers)
    {
    }
}
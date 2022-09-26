namespace Aeco.Reactive;

using System;
using System.Diagnostics.CodeAnalysis;

using Aeco.Local;

public class ReactiveCompositeLayer<TSublayer> : CompositeLayer<IComponent, TSublayer>
    where TSublayer : ILayer<IComponent>
{
    public override bool IsTerminalDataLayer => true;

    private bool _existsTemp;
    
    public ReactiveCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(entityId, out _existsTemp);
        if (_existsTemp) {
            base.Acquire<Modified<UComponent>>(entityId);
        }
        else {
            base.Acquire<Created<UComponent>>(entityId);
        }
        return ref comp;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        ref UComponent comp = ref base.Acquire<UComponent>(entityId, out exists);
        if (exists) {
            base.Acquire<Modified<UComponent>>(entityId);
        }
        else {
            base.Acquire<Created<UComponent>>(entityId);
        }
        return ref comp;
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        ref UComponent comp = ref base.Require<UComponent>(entityId);
        base.Acquire<Modified<UComponent>>(entityId);
        return ref comp;
    }

    public override void Set<UComponent>(Guid entityId, in UComponent component)
    {
        base.Set(entityId, component);
        base.Acquire<Modified<UComponent>>(entityId);
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        if (base.Remove<UComponent>(entityId)) {
            base.Acquire<Removed<UComponent>>(entityId);
            return true;
        }
        return false;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (base.Remove<UComponent>(entityId, out component)) {
            base.Acquire<Removed<UComponent>>(entityId);
            return true;
        }
        return false;
    }
}

public class ReactiveCompositeLayer : ReactiveCompositeLayer<ILayer<IComponent>>
{
    public ReactiveCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
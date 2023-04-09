namespace Aeco.Local;

using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

public class TrackedCompositeLayer<TComponent, TSublayer> : CompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    private ImmutableDictionary<uint, ImmutableHashSet<Type>> _componentTrack =
        ImmutableDictionary<uint, ImmutableHashSet<Type>>.Empty;
    
    public TrackedCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public override ref UComponent Acquire<UComponent>(uint id)
    {
        return ref base.Acquire<UComponent>(id, out bool exsts);
    }

    public override ref UComponent Acquire<UComponent>(uint id, out bool exists)
    {
        return ref base.Acquire<UComponent>(id, out exists);
    }

    public override ref UComponent AcquireRaw<UComponent>(uint id)
    {
        return ref base.AcquireRaw<UComponent>(id);
    }

    public override ref UComponent AcquireRaw<UComponent>(uint id, out bool exists)
    {
        return ref base.AcquireRaw<UComponent>(id, out exists);
    }

    public override bool Remove<UComponent>(uint id)
    {
        return base.Remove<UComponent>(id);
    }

    public override bool Remove<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
    {
        return base.Remove(id, out component);
    }

    public override void RemoveAll<UComponent>()
    {
        base.RemoveAll<UComponent>();
    }

    public override ref UComponent Set<UComponent>(uint id, in UComponent component)
    {
        return ref base.Set(id, component);
    }

    public override void Clear()
    {
        base.Clear();
    }

    public override void Clear(uint id)
    {
        base.Clear(id);
    }
}

public class TrackedCompositeLayer<TComponent>
    : TrackedCompositeLayer<TComponent, ILayer<TComponent>>, ICompositeLayer<TComponent>
{
    public TrackedCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class TrackedCompositeLayer : TrackedCompositeLayer<IComponent>
{
    public TrackedCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}
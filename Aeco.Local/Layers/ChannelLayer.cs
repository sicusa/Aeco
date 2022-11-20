namespace Aeco.Local;

using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class ChannelLayer<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    private Dictionary<Guid, LinkedList<object>> _channels = new();
    private Stack<LinkedList<object>> _channelPool = new();

    private bool _tempExists;

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (_channels.TryGetValue(entityId, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent foundComp) {
                    component = foundComp;
                    return true;
                }
            }
        }
        component = default;
        return false;
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        if (_channels.TryGetValue(entityId, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
                }
            }
        }
        throw new KeyNotFoundException("Component not found");
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
        => ref Acquire<UComponent>(entityId, out _tempExists);

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        LinkedListNode<object> node;

        if (!_channels.TryGetValue(entityId, out var channel)) {
            if (!_channelPool.TryPop(out channel)) {
                channel = new LinkedList<object>();
            }
            _channels[entityId] = channel;
            node = channel.AddFirst(new UComponent());
            exists = false;
            return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
        }

        for (node = channel.First!; node != channel.Last; node = node.Next!) {
            if (node!.Value is UComponent) {
                exists = true;
                return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
            }
        }

        node = channel.AddFirst(new UComponent());
        exists = false;
        return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
    }

    public override bool Contains<UComponent>(Guid entityId)
    {
        if (_channels.TryGetValue(entityId, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    return true;
                }
            }
        }
        return false;
    }

    public override bool ContainsAny<UComponent>()
    {
        foreach (var (_, channel) in _channels) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    return true;
                }
            }
        }
        return false;
    }

    public override Guid? Singleton<UComponent>()
    {
        foreach (var (entityId, channel) in _channels) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    return entityId;
                }
            }
        }
        return null;
    }

    public override int GetCount()
        => _channels.Select(t => t.Value.Count).Sum();

    public override int GetCount<UComponent>()
        => _channels.SelectMany(t => t.Value).OfType<UComponent>().Count();

    public override IEnumerable<Guid> Query<UComponent>()
        => throw new NotSupportedException("Query not supported for channel component");

    public override IEnumerable<Guid> Query()
        => throw new NotSupportedException("Query not supported for channel component");

    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
    {
        LinkedListNode<object> node;

        if (!_channels.TryGetValue(entityId, out var channel)) {
            channel = new LinkedList<object>();
            _channels[entityId] = channel;
            node = channel.AddFirst(component!);
            return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
        }

        node = channel.AddFirst(component!);
        return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        if (_channels.TryGetValue(entityId, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    channel.Remove(node);
                    return true;
                }
            }
        }
        return false;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (_channels.TryGetValue(entityId, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent foundComp) {
                    channel.Remove(node);
                    component = foundComp;
                    return true;
                }
            }
        }
        component = default;
        return false;
    }

    public override void RemoveAll<UComponent>()
    {
        foreach (var (_, channel) in _channels) {
            for (var node = channel.First; node != channel.Last;) {
                var next = node!.Next;
                if (node!.Value is UComponent) {
                    channel.Remove(node);
                }
                node = next;
            }
        }
    }

    public override IEnumerable<object> GetAll(Guid entityId)
        => _channels.TryGetValue(entityId, out var channel) ? channel : Enumerable.Empty<object>();

    public override void Clear(Guid entityId)
    {
        if (_channels.Remove(entityId, out var channel)) {
            channel.Clear();
            _channelPool.Push(channel);
        }
    }

    public override void Clear()
    {
        foreach (var (_, channel) in _channels) {
            channel.Clear();
            _channelPool.Push(channel);
        }
        _channels.Clear();
    }
}

public class ChannelLayer<TSelectedComponent> : ChannelLayer<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
}

public class ChannelLayer : ChannelLayer<IComponent>
{
}
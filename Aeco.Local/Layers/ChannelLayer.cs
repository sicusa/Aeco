namespace Aeco.Local;

using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class ChannelLayer<TComponent, TSelectedComponent>
    : DataLayerBase<TComponent, TSelectedComponent>
    , IReadableDataLayer<TComponent>
    , ISettableDataLayer<TComponent>
    , IShrinkableDataLayer<TComponent>
    where TSelectedComponent : TComponent
{
    private Dictionary<uint, LinkedList<object>> _channels = new();
    private Stack<LinkedList<object>> _channelPool = new();

    public override bool Contains<UComponent>(uint id)
    {
        if (_channels.TryGetValue(id, out var channel)) {
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

    public override uint? Singleton<UComponent>()
    {
        foreach (var (id, channel) in _channels) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    return id;
                }
            }
        }
        return null;
    }

    public override int GetCount()
        => _channels.Select(t => t.Value.Count).Sum();

    public override int GetCount<UComponent>()
        => _channels.SelectMany(t => t.Value).OfType<UComponent>().Count();

    public override IEnumerable<uint> Query<UComponent>()
        => throw new NotSupportedException("Query not supported for channel component");

    public override IEnumerable<uint> Query()
        => throw new NotSupportedException("Query not supported for channel component");

    public ref readonly UComponent InspectOrNullRef<UComponent>(uint id)
        where UComponent : TComponent
        => ref RequireOrNullRef<UComponent>(id);

    public bool TryGet<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        if (_channels.TryGetValue(id, out var channel)) {
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

    public ref UComponent RequireOrNullRef<UComponent>(uint id)
        where UComponent : TComponent
    {
        if (_channels.TryGetValue(id, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
                }
            }
        }
        return ref Unsafe.NullRef<UComponent>();
    }

    public ref UComponent Acquire<UComponent>(uint id)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id, out bool _);

    public ref UComponent Acquire<UComponent>(uint id, out bool exists)
        where UComponent : TComponent, new()
    {
        LinkedListNode<object> node;

        if (!_channels.TryGetValue(id, out var channel)) {
            if (!_channelPool.TryPop(out channel)) {
                channel = new LinkedList<object>();
            }
            _channels[id] = channel;
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

    public ref UComponent Set<UComponent>(uint id, in UComponent component)
        where UComponent : TComponent, new()
    {
        LinkedListNode<object> node;

        if (!_channels.TryGetValue(id, out var channel)) {
            channel = new LinkedList<object>();
            _channels[id] = channel;
            node = channel.AddFirst(component!);
            return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
        }

        node = channel.AddFirst(component!);
        return ref Unsafe.As<object, UComponent>(ref node.ValueRef);
    }

    public bool Remove<UComponent>(uint id)
        where UComponent : TComponent
    {
        if (_channels.TryGetValue(id, out var channel)) {
            for (var node = channel.First; node != channel.Last; node = node!.Next) {
                if (node!.Value is UComponent) {
                    channel.Remove(node);
                    return true;
                }
            }
        }
        return false;
    }

    public bool Remove<UComponent>(uint id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        if (_channels.TryGetValue(id, out var channel)) {
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

    public void RemoveAll<UComponent>()
        where UComponent : TComponent
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

    public IEnumerable<object> GetAll(uint id)
        => _channels.TryGetValue(id, out var channel) ? channel : Enumerable.Empty<object>();

    public void Clear(uint id)
    {
        if (_channels.Remove(id, out var channel)) {
            channel.Clear();
            _channelPool.Push(channel);
        }
    }

    public void Clear()
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
namespace Aeco.Local;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

public interface IChannel : IComponent, IDisposable
{
    Queue<Guid> Messages { get; set; }
}

[DataContract]
public class Channel<TMessage> : IChannel
{
    [DataMember]
    public Queue<Guid> Messages { get; set; } = new();
    public Channel() {}

    public void Dispose()
    {
        Messages.Clear();
    }
}

public class ChannelLayer<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public IDataLayer<IChannel> ChannelDataLayer { get; init; }
    public IDataLayer<TComponent> MessageDataLayer { get; init; }

    public ChannelLayer()
        : this(new PolyHashStorage<TComponent, TSelectedComponent>())
    {
    }

    public ChannelLayer(IDataLayer<TComponent> messageDataLayer)
        : this(new PolyHashStorage<IChannel>(), messageDataLayer)
    {
    }

    public ChannelLayer(IDataLayer<IChannel> channelDataLayer, IDataLayer<TComponent> messageDataLayer)
    {
        ChannelDataLayer = channelDataLayer;
        MessageDataLayer = messageDataLayer;
    }

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (ChannelDataLayer.TryGet<Channel<UComponent>>(entityId, out var channel)
                && channel.Messages.TryPeek(out var messageId)) {
            return MessageDataLayer.TryGet<UComponent>(messageId, out component);
        }
        component = default(UComponent);
        return false;
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        ref var channel = ref ChannelDataLayer.Require<Channel<UComponent>>(entityId);
        var messageId = channel.Messages.Peek();
        return ref MessageDataLayer.Require<UComponent>(messageId);
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        ref var channel = ref ChannelDataLayer.Acquire<Channel<UComponent>>(entityId);
        if (!channel.Messages.TryPeek(out var messageId)) {
            messageId = Guid.NewGuid();
            channel.Messages.Enqueue(messageId);
        }
        return ref MessageDataLayer.Acquire<UComponent>(messageId);
    }

    public override bool Contains<UComponent>(Guid entityId)
    {
        try {
            ref var channel = ref ChannelDataLayer.Require<Channel<UComponent>>(entityId);
            return channel.Messages.Count != 0;
        }
        catch {
            return false;
        }
    }

    public override Guid Singleton<UComponent>()
    {
        var id = ChannelDataLayer.Singleton<Channel<UComponent>>();
        if (ChannelDataLayer.Require<Channel<UComponent>>(id).Messages.Count == 0) {
            throw new KeyNotFoundException("Singleton not found");
        }
        return id;
    }

    public override IEnumerable<Guid> Query<UComponent>()
        => ChannelDataLayer.Query<Channel<UComponent>>()
            .Where(id => ChannelDataLayer.Require<Channel<UComponent>>(id).Messages.Count != 0);

    public override void Set<UComponent>(Guid entityId, in UComponent component)
    {
        ref var channel = ref ChannelDataLayer.Acquire<Channel<UComponent>>(entityId);
        var messageId = Guid.NewGuid();
        channel.Messages.Enqueue(messageId);
        MessageDataLayer.Set<UComponent>(messageId, component);
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        if (ChannelDataLayer.TryGet<Channel<UComponent>>(entityId, out var channel)
                && channel.Messages.TryDequeue(out var messageId)) {
            return MessageDataLayer.Remove<UComponent>(messageId);
        }
        return false;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (ChannelDataLayer.TryGet<Channel<UComponent>>(entityId, out var channel)
                && channel.Messages.TryDequeue(out var messageId)) {
            return MessageDataLayer.Remove<UComponent>(messageId, out component);
        }
        component = default(UComponent);
        return false;
    }

    public virtual bool RemoveChannel<UComponent>(Guid entityId)
        where UComponent : IComponent
    {
        if (!ChannelDataLayer.TryGet<Channel<UComponent>>(entityId, out var channel)) {
            return false;
        }
        foreach (var messageId in channel.Messages) {
            MessageDataLayer.Clear(messageId);
        }
        ChannelDataLayer.Remove<Channel<UComponent>>(entityId);
        return true;
    }

    public override IEnumerable<object> GetAll(Guid entityId)
        => ChannelDataLayer.GetAll(entityId)
            .Select(comp => comp as IChannel)
            .Where(channel => channel != null)
            .SelectMany(channel => channel!.Messages)
            .Select(messageId => MessageDataLayer.GetAll(messageId).First());

    public override void Clear(Guid entityId)
    {
        foreach (var comp in ChannelDataLayer.GetAll(entityId)) {
            if (comp is not IChannel channel) {
                continue;
            }
            foreach (var messageId in channel.Messages) {
                MessageDataLayer.Clear(messageId);
            }
            channel.Messages.Clear();
        }
    }
}

public class ChannelLayer<TSelectedComponent> : ChannelLayer<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
    public ChannelLayer()
    {
    }

    public ChannelLayer(IDataLayer<IComponent> messageDataLayer)
        : base(messageDataLayer)
    {
    }

    public ChannelLayer(IDataLayer<IChannel> channelDataLayer, IDataLayer<IComponent> messageDataLayer)
        : base(channelDataLayer, messageDataLayer)
    {
    }
}

public class ChannelLayer : ChannelLayer<IComponent>
{
    public ChannelLayer()
    {
    }

    public ChannelLayer(IDataLayer<IComponent> messageDataLayer)
        : base(messageDataLayer)
    {
    }

    public ChannelLayer(IDataLayer<IChannel> channelDataLayer, IDataLayer<IComponent> messageDataLayer)
        : base(channelDataLayer, messageDataLayer)
    {
    }
}
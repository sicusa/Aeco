namespace Aeco.Local;

public class PooledChannelLayer<TComponent, TSelectedComponent> : ChannelLayer<TComponent, TSelectedComponent>
    where TSelectedComponent : class, TComponent, IDisposable
{
    public PooledChannelLayer(int capacity = MonoPoolStorage.kDefaultCapacity)
        : base(new PolyPoolStorage<TComponent, TSelectedComponent>(capacity))
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, new PolyPoolStorage<TComponent, TSelectedComponent>(capacity))
    {
    }
}

public class PooledChannelLayer<TSelectedComponent> : PooledChannelLayer<object, TSelectedComponent>
    where TSelectedComponent : class, IDisposable
{
    public PooledChannelLayer(int capacity = MonoPoolStorage.kDefaultCapacity)
        : base(capacity)
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, capacity)
    {
    }
}

public class PooledChannelLayer : PooledChannelLayer<ICommand>
{
    public PooledChannelLayer(int capacity = MonoPoolStorage.kDefaultCapacity)
        : base(capacity)
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, capacity)
    {
    }
}
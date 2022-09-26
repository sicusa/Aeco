namespace Aeco.Local;

public class PooledChannelLayer<TComponent, TSelectedComponent> : ChannelLayer<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable
{
    public PooledChannelLayer(int capacity = MonoPoolStorage.DefaultCapacity)
        : base(new PolyPoolStorage<TComponent, TSelectedComponent>(capacity))
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, new PolyPoolStorage<TComponent, TSelectedComponent>(capacity))
    {
    }
}

public class PooledChannelLayer<TSelectedComponent> : PooledChannelLayer<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable
{
    public PooledChannelLayer(int capacity = MonoPoolStorage.DefaultCapacity)
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
    public PooledChannelLayer(int capacity = MonoPoolStorage.DefaultCapacity)
        : base(capacity)
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, capacity)
    {
    }
}
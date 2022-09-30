namespace Aeco.Local;

public class PooledChannelLayer<TComponent, TSelectedComponent> : ChannelLayer<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable
{
    public PooledChannelLayer(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
        : base(new PolyPoolStorage<TComponent, TSelectedComponent>(brickCapacity))
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
    public PooledChannelLayer(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
        : base(brickCapacity)
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, capacity)
    {
    }
}

public class PooledChannelLayer : PooledChannelLayer<ICommand>
{
    public PooledChannelLayer(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
        : base(brickCapacity)
    {
    }

    public PooledChannelLayer(IDataLayer<IChannel> channelDataLayer, int capacity)
        : base(channelDataLayer, capacity)
    {
    }
}
namespace Aeco.Local;

public class PolyPoolStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable
{
    public PolyPoolStorage()
        : this(MonoPoolStorage.DefaultBrickCapacity)
    {
    }
    public PolyPoolStorage(int brickCapacity)
        : base(MonoPoolStorage.MakeUnsafeCreator<TComponent>(brickCapacity))
    {
    }
}

public class PolyPoolStorage<TSelectedComponent> : PolyPoolStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable
{
    public PolyPoolStorage()
        : this(MonoPoolStorage.DefaultBrickCapacity)
    {
    }
    public PolyPoolStorage(int brickCapacity)
        : base(brickCapacity)
    {
    }
}
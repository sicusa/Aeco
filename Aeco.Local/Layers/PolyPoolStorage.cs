namespace Aeco.Local;

public class PolyPoolStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable
{
    public PolyPoolStorage(int brickCapacity = MonoPoolStorage.DefaultBrickCapacity)
        : base(MonoPoolStorage.MakeUnsafeCreator<TComponent>(brickCapacity))
    {
    }
}

public class PolyPoolStorage<TSelectedComponent> : PolyPoolStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable
{
    public PolyPoolStorage(int capacity = MonoPoolStorage.DefaultBrickCapacity)
        : base(capacity)
    {
    }
}
namespace Aeco.Local;

public class PolyPoolStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable
{
    public PolyPoolStorage(int capacity = MonoPoolStorage.DefaultCapacity)
        : base(MonoPoolStorage.MakeUnsafeCreator<TComponent>(capacity))
    {
    }
}

public class PolyPoolStorage<TSelectedComponent> : PolyPoolStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable
{
    public PolyPoolStorage(int capacity = MonoPoolStorage.DefaultCapacity)
        : base(capacity)
    {
    }
}
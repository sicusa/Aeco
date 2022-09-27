namespace Aeco.Local;

public class PolySingletonStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable
{
    public PolySingletonStorage()
        : base(SingletonStorage.MakeUnsafeCreator<TComponent>())
    {
    }
}

public class PolySingletonStorage<TSelectedComponent> : PolySingletonStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable
{
}
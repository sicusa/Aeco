namespace Aeco.Local;

public class PolySingletonStorage<TComponent, TSelectedComponent> : PolyStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public PolySingletonStorage()
        : base(SingletonStorage.Factory<TComponent>.Shared)
    {
    }
}

public class PolySingletonStorage<TSelectedComponent> : PolySingletonStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
}
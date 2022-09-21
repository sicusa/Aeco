namespace Aeco.Local;

public class PolyHashStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public PolyHashStorage()
        : base(MonoHashStorage.MakeUnsafeCreator<TComponent>())
    {
    }
}

public class PolyHashStorage<TSelectedComponent> : PolyHashStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
}

public class PolyHashStorage : PolyHashStorage<IComponent>
{
}
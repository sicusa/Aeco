namespace Aeco.Local;

public class PolyTagStorage<TComponent, TSelectedComponent> : PolyStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public PolyTagStorage()
        : base(MonoTagStorage.Factory<TComponent>.Shared)
    {
    }
}

public class PolyTagStorage<TSelectedComponent> : PolyTagStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
}
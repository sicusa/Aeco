namespace Aeco.Local;

public class PolyHashStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public PolyHashStorage()
        : base(MonoHashStorage.MakeUnsafeCreator<TComponent>())
    {
    }
}

public class PolyHashStorage<TSelectedComponent> : PolyHashStorage<object, TSelectedComponent>
    where TSelectedComponent: notnull
{
}

public class PolyHashStorage : PolyHashStorage<object>
{
}
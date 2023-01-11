namespace Aeco.Local;

public class PolyClosedHashStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public PolyClosedHashStorage()
        : base(MonoClosedHashStorage.Factory<TComponent>.Default)
    {
    }
    public PolyClosedHashStorage(int brickCapacity)
        : base(new MonoClosedHashStorage.Factory<TComponent>() {
            BrickCapacity = brickCapacity
        })
    {
    }
}

public class PolyClosedHashStorage<TSelectedComponent> : PolyClosedHashStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
    public PolyClosedHashStorage()
        : this(MonoClosedHashStorage.DefaultBrickCapacity)
    {
    }
    public PolyClosedHashStorage(int brickCapacity)
        : base(brickCapacity)
    {
    }
}
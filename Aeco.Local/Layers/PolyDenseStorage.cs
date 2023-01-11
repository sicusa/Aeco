namespace Aeco.Local;

public class PolyDenseStorage<TComponent, TSelectedComponent> : CompositeStorage<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public PolyDenseStorage()
        : base(MonoDenseStorage.Factory<TComponent>.Default)
    {
    }
    public PolyDenseStorage(int pageCount, int pageSize)
        : base(new MonoDenseStorage.Factory<TComponent> {
            PageCount = pageCount,
            PageSize = pageSize
        })
    {
    }
}

public class PolyDenseStorage<TSelectedComponent> : PolyDenseStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
    public PolyDenseStorage()
        : this(MonoDenseStorage.DefaultPageCount, MonoDenseStorage.DeafultPageSize)
    {
    }
    public PolyDenseStorage(int pageCount, int pageSize)
        : base(pageCount, pageSize)
    {
    }
}
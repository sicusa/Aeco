namespace Aeco;

public class VirtualLayer<TComponent> : ILayer<TComponent>
{
    public IReadOnlyEntity<TComponent> GetReadOnlyEntity(Guid id)
        => throw new NotImplementedException();
    public IEntity<TComponent> GetEntity(Guid id)
        => throw new NotImplementedException();
}

public class VirtualLayer : VirtualLayer<IComponent>
{
}
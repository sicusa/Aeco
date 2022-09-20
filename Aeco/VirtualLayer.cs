namespace Aeco;

public class VirtualLayer<TComponent> : ILayer<TComponent>
{
    public IEntity<TComponent> GetEntity(Guid id)
        => throw new NotImplementedException();
}

public class VirtualLayer : VirtualLayer<object>
{
}
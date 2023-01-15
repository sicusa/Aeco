namespace Aeco;

public struct ComponentRef<TComponent>
{
    public Guid Id { get; private init; }
    public bool IsValid => _host.IsRefValid(Id);

    private IComponentRefHost<TComponent> _host;

    public ComponentRef(IComponentRefHost<TComponent> host, Guid id)
    {
        _host = host;
        Id = id;
    }

    public ref TComponent GetRef()
        => ref _host.RequireRef(Id);
}
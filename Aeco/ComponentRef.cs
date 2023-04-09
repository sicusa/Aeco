namespace Aeco;

public struct ComponentRef<TComponent>
{
    public uint Id { get; private init; }
    public bool IsValid => _host.IsRefValid(Id, _internalId);

    private IComponentRefHost<TComponent> _host;
    private int _internalId;

    public ComponentRef(IComponentRefHost<TComponent> host, uint id, int internalId = 0)
    {
        Id = id;
        _host = host;
        _internalId = internalId;
    }

    public ref TComponent GetRef()
        => ref _host.RequireRef(Id, _internalId);
}
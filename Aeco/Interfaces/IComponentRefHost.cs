namespace Aeco;

public interface IComponentRefHost<TComponent>
{
    bool IsRefValid(Guid id, int internalId);
    ref TComponent RequireRef(Guid id, int internalId);
}
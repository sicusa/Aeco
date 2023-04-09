namespace Aeco;

public interface IComponentRefHost<TComponent>
{
    bool IsRefValid(uint id, int internalId);
    ref TComponent RequireRef(uint id, int internalId);
}
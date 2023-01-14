namespace Aeco;

public interface IComponentRefHost<TComponent>
{
    bool IsRefValid(Guid refId);
    ref TComponent RequireRef(Guid refId);
}
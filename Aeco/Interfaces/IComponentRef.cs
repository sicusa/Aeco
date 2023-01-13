namespace Aeco;

public interface IComponentRef<TComponent>
{
    Guid Id { get; }
    bool IsValid { get; }

    ref TComponent GetRef();
}
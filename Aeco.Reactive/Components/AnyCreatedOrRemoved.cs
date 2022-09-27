namespace Aeco.Reactive;

public struct AnyCreatedOrRemoved<TComponent> : IReactiveEvent<TComponent>
{
    public void Dispose()
    {
    }
}
namespace Aeco.Reactive;

public struct Removed<TComponent> : IReactiveEvent<TComponent>
{
    public void Dispose()
    {
    }
}
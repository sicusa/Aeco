namespace Aeco.Reactive;

public struct AnyRemoved<TComponent> : IReactiveEvent<TComponent>
{
    public void Dispose()
    {
    }
}
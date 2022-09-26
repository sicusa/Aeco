namespace Aeco.Reactive;

public interface IReactiveEvent : IComponent, IDisposable
{
}

public interface IReactiveEvent<out TComponent> : IReactiveEvent
{
}
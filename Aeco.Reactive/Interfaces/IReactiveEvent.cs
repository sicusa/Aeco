namespace Aeco.Reactive;

public interface IReactiveEvent : IComponent
{
}

public interface IReactiveEvent<out TComponent> : IReactiveEvent
{
}
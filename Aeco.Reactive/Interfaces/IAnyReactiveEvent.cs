namespace Aeco.Reactive;

public interface IAnyReactiveEvent : IReactiveEvent
{
}

public interface IAnyReactiveEvent<out TComponent> : IAnyReactiveEvent, IReactiveEvent<TComponent>
{
}
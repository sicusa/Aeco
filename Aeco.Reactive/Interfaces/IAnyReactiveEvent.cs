namespace Aeco.Reactive;

public interface IAnyReactiveEvent : IComponent
{
}

public interface IAnyReactiveEvent<out TComponent> : IAnyReactiveEvent
{
}
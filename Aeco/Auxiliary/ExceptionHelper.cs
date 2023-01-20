namespace Aeco;

public static class ExceptionHelper
{
    public static Exception ComponentNotFound<TComponent>()
        => new KeyNotFoundException("Component not found: " + typeof(TComponent));

    public static Exception ComponentNotSupported<TComponent>()
        => new NotSupportedException("Component not supported: " + typeof(TComponent));
}
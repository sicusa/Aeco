namespace Aeco;

public static class ExceptionHelper
{
    public static Exception ComponentNotFound<TComponent>()
        => new KeyNotFoundException("Component not found: " + typeof(TComponent));
}
namespace Aeco;

public class Layer<TComponent> : ILayer<TComponent>
{
}

public class Layer : Layer<IComponent>
{
}
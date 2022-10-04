namespace Aeco.Renderer.GL;

using Aeco.Local;

public class AutoClearCompositeLayer : CompositeLayer, IGLLateUpdateLayer
{
    public AutoClearCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }

    public void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
        => Clear();
}
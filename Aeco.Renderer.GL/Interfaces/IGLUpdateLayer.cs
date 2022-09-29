namespace Aeco.Renderer.GL;

public interface IGLUpdateLayer : ILayer<IComponent>
{
    void OnUpdate(IDataLayer<IComponent> context, float deltaTime);
}
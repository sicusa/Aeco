namespace Aeco.Renderer.GL;

public interface IGLLateUpdateLayer : ILayer<IComponent>
{
    void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime);
}
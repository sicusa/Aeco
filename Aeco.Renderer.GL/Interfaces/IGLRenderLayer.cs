namespace Aeco.Renderer.GL;

public interface IGLRenderLayer : ILayer<IComponent>
{
    void OnRender(IDataLayer<IComponent> context, float deltaTime);
}
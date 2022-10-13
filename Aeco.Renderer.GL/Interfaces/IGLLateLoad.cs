namespace Aeco.Renderer.GL;

public interface IGLLateLoadLayer : ILayer<IComponent>
{
    void OnLateLoad(IDataLayer<IComponent> context);
}
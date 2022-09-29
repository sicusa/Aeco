namespace Aeco.Renderer.GL;

public interface IGLLoadLayer : ILayer<IComponent>
{
    void OnLoad(IDataLayer<IComponent> context);
}
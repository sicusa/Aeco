namespace Aeco.Renderer.GL;

public interface IGLResizeLayer : ILayer<IComponent>
{
    void OnResize(IDataLayer<IComponent> context, int width, int height);
}
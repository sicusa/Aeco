namespace Aeco.Renderer.GL;

public interface IGLUnloadLayer : ILayer<IComponent>
{
    void OnUnload(IDataLayer<IComponent> context);
}
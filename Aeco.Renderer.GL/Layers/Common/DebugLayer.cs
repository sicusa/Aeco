namespace Aeco.Renderer.GL;

public class DebugLayer : VirtualLayer, IGLLoadLayer, IGLUnloadLayer, IGLUpdateLayer, IGLLateUpdateLayer, IGLRenderLayer
{
    public event Action? OnLoad;
    public event Action? OnUnload;
    public event Action<float>? OnUpdate;
    public event Action<float>? OnLateUpdate;
    public event Action<float>? OnRender;

    void IGLLoadLayer.OnLoad(IDataLayer<IComponent> context)
        => OnLoad?.Invoke();

    void IGLUnloadLayer.OnUnload(IDataLayer<IComponent> context)
        => OnUnload?.Invoke();

    void IGLUpdateLayer.OnUpdate(IDataLayer<IComponent> context, float deltaTime)
        => OnUpdate?.Invoke(deltaTime);

    void IGLLateUpdateLayer.OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
        => OnLateUpdate?.Invoke(deltaTime);

    void IGLRenderLayer.OnRender(IDataLayer<IComponent> context, float deltaTime)
        => OnRender?.Invoke(deltaTime);
}
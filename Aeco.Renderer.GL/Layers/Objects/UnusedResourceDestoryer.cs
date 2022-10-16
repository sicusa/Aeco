namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public class UnusedResourceDestroyer : VirtualLayer, IGLLateUpdateLayer
{
    private Group<ResourceReferencers> _g = new();

    public void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _g.Query(context)) {
            if (context.Inspect<ResourceReferencers>(id).Ids.Count == 0) {
                context.Acquire<Destroy>(id);
            }
        }
    }
}
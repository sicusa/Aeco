namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public abstract class UninitializerBase<TObject, TObjectData> : VirtualLayer, IGLLateUpdateLayer, IGLUnloadLayer
    where TObject : IComponent
    where TObjectData : IComponent
{
    public void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<TObject>>()) {
            Uninitialize(context, id);
        }
    }

    public void OnUnload(IDataLayer<IComponent> context)
    {
        foreach (var id in context.Query<TObjectData>().ToArray()) {
            Uninitialize(context, id);
        }
    }

    private void Uninitialize(IDataLayer<IComponent> context, Guid id)
    {
        if (context.Remove<TObjectData>(id, out var handle)) {
            Uninitialize(handle);
        }
    }

    protected abstract void Uninitialize(in TObjectData data);
}
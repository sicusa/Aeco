namespace Aeco.Renderer.GL;

public struct TransformMatricesDirty : IGLObject
{
    public void Dispose() { }

    public static void Tag(IDataLayer<IComponent> context, Guid id)
    {
        context.Acquire<TransformMatricesDirty>(id);

        var currId = id;
        while (context.TryGet<Parent>(currId, out var parent)) {
            context.Acquire<ChildrenTransformMatricesDirty>(parent.Id);
            currId = parent.Id;
        }
    }
}
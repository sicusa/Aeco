namespace Aeco;

public interface IParentLayerListener<in TComponent, in TParentLayer>
    where TParentLayer : ILayer<TComponent>
{
    void OnLayerAdded(TParentLayer parent);
    void OnLayerRemoved(TParentLayer parent);
}


public interface IParentLayerListener<TComponent>
    : IParentLayerListener<TComponent, ILayer<TComponent>>
{
}
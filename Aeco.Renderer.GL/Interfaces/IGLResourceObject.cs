namespace Aeco.Renderer.GL;

public interface IGLResourceObject<TResource> : IGLReactiveObject
    where TResource : IGLResource
{
    TResource Resource { get; set; }
}
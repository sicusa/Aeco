namespace Aeco.Renderer.GL;

public class DefaultTextureLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        ref var texture = ref context.Acquire<Texture>(GLRenderer.DefaultTextureId);
        texture.Resource = TextureResource.White;
        Console.WriteLine("Default texture loaded: " + GLRenderer.DefaultTextureId);
    }
}
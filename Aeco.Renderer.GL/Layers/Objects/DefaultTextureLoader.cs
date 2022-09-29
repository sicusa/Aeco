namespace Aeco.Renderer.GL;

using System.Reflection;

public class DefaultTextureLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var stream = assembly.GetManifestResourceStream("Aeco.Renderer.GL.Embeded.Textures.wall.jpg");
        if (stream == null) {
            throw new FileNotFoundException("Failed to load default texture");
        }

        ref var texture = ref context.Acquire<Texture>(GLRendererCompositeLayer.DefaultTextureId);
        texture.Stream = stream;
        context.Acquire<DefaultShaderProgram>(GLRendererCompositeLayer.DefaultTextureId);
        Console.WriteLine("Default texture loaded.");
    }
}
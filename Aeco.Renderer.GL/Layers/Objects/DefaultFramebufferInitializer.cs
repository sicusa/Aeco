namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

public class DefaultFramebufferLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        ref var framebuffer = ref context.Acquire<Framebuffer>(GLRenderer.DefaultFramebufferId);
        framebuffer.Resource = FramebufferResource.Default;
        Console.WriteLine("Default framebuffer initialized: " + GLRenderer.DefaultFramebufferId);
    }
}
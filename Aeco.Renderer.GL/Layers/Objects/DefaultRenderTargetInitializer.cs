namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

public class DefaultRenderTargetLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        ref var renderTarget = ref context.Acquire<RenderTarget>(GLRenderer.DefaultRenderTargetId);
        renderTarget.Resource = RenderTargetResource.AutoResized;
        Console.WriteLine("Default render target initialized: " + GLRenderer.DefaultRenderTargetId);
    }
}
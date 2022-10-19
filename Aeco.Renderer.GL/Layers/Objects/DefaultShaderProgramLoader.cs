namespace Aeco.Renderer.GL;

using System.Reflection;

public class DefaultShaderProgramLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = new ShaderProgramResource();

        resource.Shaders[ShaderType.Vertex] =
            InternalAssets.Load<TextResource>("Shaders.vert.glsl").Content;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.frag.glsl").Content;

        ref var program = ref context.Acquire<ShaderProgram>(GLRenderer.DefaultShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Default texture loaded.");
    }
}
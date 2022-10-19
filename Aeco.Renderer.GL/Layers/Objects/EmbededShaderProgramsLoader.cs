namespace Aeco.Renderer.GL;

using System.Reflection;

public class EmbededShaderProgramsLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        // load default shader program

        var resource = new ShaderProgramResource();

        resource.Shaders[ShaderType.Vertex] =
            InternalAssets.Load<TextResource>("Shaders.vert.glsl").Content;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.frag.glsl").Content;

        ref var program = ref context.Acquire<ShaderProgram>(GLRenderer.DefaultShaderProgramId);
        program.Resource = resource;

        Console.WriteLine("Default shader program loaded: " + GLRenderer.DefaultShaderProgramId);

        // load culling shader program

        resource = new ShaderProgramResource();
        
        resource.Shaders[ShaderType.Vertex] =
            InternalAssets.Load<TextResource>("Shaders.cull.vert.glsl").Content;
        resource.Shaders[ShaderType.Geometry] =
            InternalAssets.Load<TextResource>("Shaders.cull.geo.glsl").Content;
        resource.TransformFeedbackVaryings = new string[] { "culledObjectToWorld" };

        program = ref context.Acquire<ShaderProgram>(GLRenderer.CullingShaderProgramId);
        program.Resource = resource;

        Console.WriteLine("Culling shader program loaded: " + GLRenderer.CullingShaderProgramId);
    }
}
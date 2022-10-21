namespace Aeco.Renderer.GL;

public class EmbededShaderProgramsLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        // load default shader program

        var resource = new ShaderProgramResource();
        var emptyVertShader = InternalAssets.Load<TextResource>("Shaders.empty.vert.glsl").Content;

        resource.Shaders[ShaderType.Vertex] =
            InternalAssets.Load<TextResource>("Shaders.blinn_phong.vert.glsl").Content;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.blinn_phong.frag.glsl").Content;

        ref var program = ref context.Acquire<ShaderProgram>(GLRenderer.DefaultShaderProgramId);
        program.Resource = resource;

        Console.WriteLine("Default shader program loaded: " + GLRenderer.DefaultShaderProgramId);

        // load culling shader program

        resource = new ShaderProgramResource { IsMaterialTexturesEnabled = false };
        
        resource.Shaders[ShaderType.Vertex] =
            InternalAssets.Load<TextResource>("Shaders.cull.vert.glsl").Content;
        resource.Shaders[ShaderType.Geometry] =
            InternalAssets.Load<TextResource>("Shaders.cull.geo.glsl").Content;
        resource.TransformFeedbackVaryings = new string[] { "CulledObjectToWorld" };

        program = ref context.Acquire<ShaderProgram>(GLRenderer.CullingShaderProgramId);
        program.Resource = resource;

        Console.WriteLine("Culling shader program loaded: " + GLRenderer.CullingShaderProgramId);

        // load hierarchical-Z shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false,
            CustomUniforms = new() { "LastMipSize" }
        };
        
        resource.Shaders[ShaderType.Vertex] = emptyVertShader;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.hiz.frag.glsl").Content;
        resource.Shaders[ShaderType.Geometry] =
            InternalAssets.Load<TextResource>("Shaders.hiz.geo.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.HierarchicalZShaderProgramId);
        program.Resource = resource;

        Console.WriteLine("Hierarchical-Z shader program loaded: " + GLRenderer.HierarchicalZShaderProgramId);
    }
}
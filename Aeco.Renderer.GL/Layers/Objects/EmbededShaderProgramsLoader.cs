namespace Aeco.Renderer.GL;

public class EmbededShaderProgramsLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        var emptyVertShader = InternalAssets.Load<TextResource>("Shaders.empty.vert.glsl").Content;
        var simpleVertShader = InternalAssets.Load<TextResource>("Shaders.simple.vert.glsl").Content;
        var whiteFragShader = InternalAssets.Load<TextResource>("Shaders.white.frag.glsl").Content;
        var quadGeoShader = InternalAssets.Load<TextResource>("Shaders.quad.geo.glsl").Content;
        var tilesGeoShader = InternalAssets.Load<TextResource>("Shaders.tiles.geo.glsl").Content;

        // load default shader program

        var resource = new ShaderProgramResource();
        var blinnPhongVert = InternalAssets.Load<TextResource>("Shaders.blinn_phong.vert.glsl").Content;

        resource.Shaders[ShaderType.Vertex] = blinnPhongVert;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.blinn_phong.frag.glsl").Content;

        ref var program = ref context.Acquire<ShaderProgram>(GLRenderer.DefaultOpaqueProgramId);
        program.Resource = resource;
        Console.WriteLine("Default shader program loaded: " + GLRenderer.DefaultOpaqueProgramId);

        // load default transparent shader program

        resource = new ShaderProgramResource();

        resource.Shaders[ShaderType.Vertex] = blinnPhongVert;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.transparent_blinn_phong.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.DefaultTransparentShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Default transparent shader program loaded: " + GLRenderer.DefaultTransparentShaderProgramId);

        // load motion shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false
        };
        resource.Shaders[ShaderType.Vertex] = simpleVertShader;
        resource.Shaders[ShaderType.Geometry] = quadGeoShader;
        resource.Shaders[ShaderType.Fragment] = 
            InternalAssets.Load<TextResource>("Shaders.motion.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.MotionShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Motion shader program loaded: " + GLRenderer.MotionShaderProgramId);

        // load culling shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false
        };

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
            CustomUniforms = new[] { "LastMip", "LastMipSize" },
            Subroutines = new() {
                [ShaderType.Fragment] = new[] { "GenerateMax", "GenerateMin" }
            }
        };

        resource.Shaders[ShaderType.Vertex] = emptyVertShader;
        resource.Shaders[ShaderType.Geometry] = quadGeoShader;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.hiz.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.HierarchicalZShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Hierarchical-Z shader program loaded: " + GLRenderer.HierarchicalZShaderProgramId);

        // load blit shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false
        };

        resource.Shaders[ShaderType.Vertex] = emptyVertShader;
        resource.Shaders[ShaderType.Geometry] = quadGeoShader;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.blit.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.BlitShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Blit shader program loaded: " + GLRenderer.BlitShaderProgramId);

        // transparency compose shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false,
            CustomUniforms = new[] { "AccumColorTex", "AccumAlphaTex" }
        };

        resource.Shaders[ShaderType.Vertex] = emptyVertShader;
        resource.Shaders[ShaderType.Geometry] = quadGeoShader;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.transparency_compose.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.TransparencyComposeShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Transparency compose shader program loaded: " + GLRenderer.TransparencyComposeShaderProgramId);

        // load post-processing shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false,
            CustomUniforms = new[] {
                "ColorBuffer",
                "TransparencyAccumBuffer",
                "TransparencyAlphaBuffer",
                "MaxDepthBuffer",
                "MinDepthBuffer"
            },
            Subroutines = new() {
                [ShaderType.Fragment] = new[] {
                    "ShowColor",
                    "ShowTransparencyAccum",
                    "ShowTransparencyAlpha",
                    "ShowMaxDepth",
                    "ShowMinDepth"
                }
            }
        };

        resource.Shaders[ShaderType.Vertex] = emptyVertShader;
        resource.Shaders[ShaderType.Geometry] = quadGeoShader;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.post.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.PostProcessingShaderProgramId);
        program.Resource = resource;
        Console.WriteLine("Post-processing shader program loaded: " + GLRenderer.PostProcessingShaderProgramId);

        // load test tiles shader program

        resource = new ShaderProgramResource {
            IsMaterialTexturesEnabled = false,
            CustomUniforms = new[] { "TileSize" }
        };

        resource.Shaders[ShaderType.Vertex] = emptyVertShader;
        resource.Shaders[ShaderType.Geometry] = tilesGeoShader;
        resource.Shaders[ShaderType.Fragment] =
            InternalAssets.Load<TextResource>("Shaders.test.frag.glsl").Content;

        program = ref context.Acquire<ShaderProgram>(GLRenderer.TestShaderProgramId);
        program.Resource = resource;
    }
}
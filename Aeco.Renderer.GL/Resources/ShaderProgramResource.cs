namespace Aeco.Renderer.GL;

public enum ShaderType
{
    Fragment,
    Vertex,
    Geometry,
    TessellationEvaluation,
    TessellationControl,
    ComputeShader
}

public class ShaderProgramResource : IGLResource
{
    public readonly EnumArray<ShaderType, string?> Shaders = new();
    public string[]? TransformFeedbackVaryings;
}
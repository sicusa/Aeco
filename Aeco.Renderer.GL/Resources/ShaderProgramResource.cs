namespace Aeco.Renderer.GL;

public enum ShaderType
{
    Fragment,
    Vertex,
    Geometry,
    TessellationEvaluation,
    TessellationControl,
    Compute
}

public record ShaderProgramResource : IGLResource
{
    public readonly EnumArray<ShaderType, string?> Shaders = new();
    public string[]? TransformFeedbackVaryings;
    public bool IsMaterialTexturesEnabled = true;
    public Dictionary<string, object>? DefaultUniformValues;
    public string[]? CustomUniforms;
    public EnumArray<ShaderType, string[]?>? Subroutines;
}
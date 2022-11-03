namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct Material : IGLResourceObject<MaterialResource>
{
    public MaterialResource Resource { get; set; } = MaterialResource.Default;
    public ShaderProgramResource? ShaderProgram = null;

    public Material() {}

    public void Dispose() => this = new();
}
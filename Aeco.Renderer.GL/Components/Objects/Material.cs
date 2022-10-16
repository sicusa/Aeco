namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct Material : IGLResourceObject<MaterialResource>
{
    public MaterialResource Resource { get; set; } = MaterialResource.Default;
    public ShaderProgramResource ShaderProgram = ShaderProgramResource.Default;

    [DataMember] public Vector2 Tiling = Vector2.One;
    [DataMember] public Vector2 Offset = Vector2.Zero;

    public Material() {}

    public void Dispose() { this = new(); }
}
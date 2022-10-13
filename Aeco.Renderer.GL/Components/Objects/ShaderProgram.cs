namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct ShaderProgram : IGLResourceObject<ShaderProgramResource>
{
    public ShaderProgramResource Resource { get; set; }

    public void Dispose() { this = new(); }
}
namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Material : IGLReactiveObject
{
    [DataMember] public Guid ShaderProgram;
    [DataMember] public Guid Texture;

    public void Dispose() { this = default; }
}
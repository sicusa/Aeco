namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Light : IGLResourceObject<LightResourceBase>
{
    public LightResourceBase Resource { get; set; }

    public void Dispose() => this = new();
}
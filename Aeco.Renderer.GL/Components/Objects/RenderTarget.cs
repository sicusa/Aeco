namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct RenderTarget : IGLResourceObject<RenderTargetResource>
{
    public RenderTargetResource Resource { get; set; } = RenderTargetResource.Default;

    public RenderTarget() {}

    public void Dispose() => this = new();
}
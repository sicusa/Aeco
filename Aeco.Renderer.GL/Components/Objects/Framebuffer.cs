namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Framebuffer : IGLResourceObject<FramebufferResource>
{
    public FramebufferResource Resource { get; set; } = FramebufferResource.Default;

    public Framebuffer() {}

    public void Dispose() { this = new(); }
}
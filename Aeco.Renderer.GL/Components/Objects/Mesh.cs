namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Mesh : IGLResourceObject<MeshResource>
{
    public MeshResource Resource { get; set; } = MeshResource.Empty;
    public bool IsCompleteInstanceRefreshEnabled = false;
    
    public Mesh() {}

    public void Dispose() { this = new(); }
}
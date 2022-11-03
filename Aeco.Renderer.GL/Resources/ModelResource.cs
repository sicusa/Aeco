namespace Aeco.Renderer.GL;

using System.Numerics;

public record ModelNodeResource : IGLResource
{
    public string Name = "";
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public MeshResource[]? Meshes;
    public LightResourceBase[]? Lights;
    public ModelNodeResource[]? Children;
    public readonly Dictionary<string, object> Metadata = new();
}

public record ModelResource : IGLResource
{
    public AnimationResource[]? Animations;
    public ModelNodeResource? RootNode;
}
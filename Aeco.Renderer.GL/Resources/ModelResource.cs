namespace Aeco.Renderer.GL;

using System.Numerics;

public record ModelNodeResource : IGLResource
{
    public string Name = "";
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public MeshResource[]? Meshes;
    public ModelNodeResource[]? Children;
    public readonly Dictionary<string, object> Metadata = new();
}

public class ModelResource : IGLResource
{
    public AnimationResource[]? Animations;
    public LightResource[]? Lights;
    public ModelNodeResource? RootNode;
}
namespace Aeco.Renderer.GL;

using System.Numerics;

public class MeshResource : IGLResource
{
    public static readonly MeshResource Empty = new() {
        Material = MaterialResource.Default
    };

    public Vector3[]? Vertices;
    public Vector3[]? TexCoords;
    public Vector3[]? Normals;
    public Vector3[]? Tangents;
    public int[]? Indeces;
    public Rectangle BoudingBox;
    public MaterialResource? Material;
}
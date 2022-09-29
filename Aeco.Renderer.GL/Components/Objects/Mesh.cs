namespace Aeco.Renderer.GL;

using System.Runtime.Serialization;

[DataContract]
public struct Mesh : IGLReactiveObject
{
    [DataMember] public float[] Vertices;
    [DataMember] public uint[] Indeces;

    public void Dispose() { this = default; }
}
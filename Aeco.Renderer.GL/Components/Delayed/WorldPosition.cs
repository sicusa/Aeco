namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct WorldPosition : IGLReactiveObject, IDisposable
{
    [DataMember] public Vector3 Value;

    public void Dispose() { this = new(); }

    public static implicit operator Vector3(in WorldPosition pos)
        => pos.Value;
}
namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct Scale : IGLReactiveObject, IDisposable
{
    [DataMember] public Vector3 Value = Vector3.One;

    public Scale() {}

    public void Dispose() { this = new(); }

    public static implicit operator Vector3(in Scale scale)
        => scale.Value;
}
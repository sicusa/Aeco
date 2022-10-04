namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct WorldRotation : IGLReactiveObject, IDisposable
{
    [DataMember] public Quaternion Value = Quaternion.Identity;

    public WorldRotation() {}

    public void Dispose() { this = new(); }

    public static implicit operator Quaternion(in WorldRotation rot)
        => rot.Value;
}
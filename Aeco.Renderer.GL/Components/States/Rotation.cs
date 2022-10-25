namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct Rotation : IGLReactiveObject
{
    [DataMember] public Quaternion Value = Quaternion.Identity;

    public Rotation() {}

    public void Dispose() { this = new(); }

    public static implicit operator Quaternion(in Rotation rot)
        => rot.Value;
}
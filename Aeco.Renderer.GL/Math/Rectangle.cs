namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct Rectangle : IEquatable<Rectangle>
{
    [DataMember] public Vector3 Min;
    [DataMember] public Vector3 Max;

    public Rectangle(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public bool Equals(Rectangle other)
        => Min == other.Min && Max == other.Max;
}
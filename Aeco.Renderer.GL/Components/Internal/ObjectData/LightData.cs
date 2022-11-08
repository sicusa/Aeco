namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

public enum LightCategory
{
    None = 0,
    Ambient,
    Directional,
    Point,
    Spot,
    Area
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightParameters
{
    public static readonly int MemorySize = Marshal.SizeOf<LightParameters>();

    public float Category;

    public Vector4 Color;
    public Vector3 Position;
    public Vector3 Direction;
    public Vector3 Up;

    public float AttenuationConstant;
    public float AttenuationLinear;
    public float AttenuationQuadratic;

    public Vector2 ConeCutoffsOrAreaSize;
}

public struct LightData : IGLObject
{
    public int Id = -1;
    public LightCategory Category = LightCategory.None;
    public LightParameters Parameters = new();

    public LightData() {}

    public void Dispose() => this = new();
}
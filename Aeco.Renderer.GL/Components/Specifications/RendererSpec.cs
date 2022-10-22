namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.Serialization;

[DataContract]
public struct RendererSpec : IGLObject
{
    // Window
    [DataMember] public int Width = 800;
    [DataMember] public int Height = 600;
    [DataMember] public string Title = "Aeco Renderer";
    [DataMember] public bool IsResizable = true;
    [DataMember] public bool IsFullscreen = false;
    [DataMember] public bool HasBorder = true;
    [DataMember] public (int, int)? MaximumSize = null;
    [DataMember] public (int, int)? MinimumSize = null;
    [DataMember] public (int, int)? Location = null;

    // Rendering
    [DataMember] public int RenderFrequency = 60;
    [DataMember] public int UpdateFrequency = 60;
    [DataMember] public Vector4 ClearColor = new Vector4(0.2f, 0.3f, 0.3f, 1.0f);

    // Debug
    [DataMember] public bool IsDebugEnabled = false;
    
    public RendererSpec() {}

    public void Dispose() { this = new(); }
}
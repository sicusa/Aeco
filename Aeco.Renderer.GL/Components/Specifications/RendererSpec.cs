namespace Aeco.Renderer.GL;

using System.Numerics;

public struct RendererSpec : IComponent, IDisposable
{
    // Window
    public int Width = 800;
    public int Height = 600;
    public string Title = "Aeco Renderer";
    public bool IsResizable = true;
    public bool IsFullscreen = false;
    public bool HasBorder = true;
    public (int, int)? MaximumSize = null;
    public (int, int)? MinimumSize = null;
    public (int, int)? Location = null;

    // Rendering
    public int RenderFrequency = 60;
    public int UpdateFrequency = 60;
    public Vector4 ClearColor = new Vector4(0.2f, 0.3f, 0.3f, 1.0f);
    
    public RendererSpec() {}

    public void Dispose() { this = default; }
}
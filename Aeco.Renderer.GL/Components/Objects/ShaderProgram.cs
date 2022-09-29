namespace Aeco.Renderer.GL;

public struct ShaderProgram : IGLReactiveObject
{
    public string VertexShader;
    public string FragmentShader;

    public void Dispose() { this = default; }
}
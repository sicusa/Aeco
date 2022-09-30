namespace Aeco.Renderer.GL;

using System.Reflection;

public class DefaultShaderProgramLoader : VirtualLayer, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var vertStream = assembly.GetManifestResourceStream("Aeco.Renderer.GL.Embeded.Shaders.vert.glsl");
        if (vertStream == null) {
            throw new FileNotFoundException("Failed to load default vertex shader");
        }
        var fragStream = assembly.GetManifestResourceStream("Aeco.Renderer.GL.Embeded.Shaders.frag.glsl");
        if (fragStream == null) {
            throw new FileNotFoundException("Failed to load default fragment shader");
        }

        ref var program = ref context.Acquire<ShaderProgram>(GLRendererLayer.DefaultShaderProgramId);
        using (var reader = new StreamReader(vertStream, System.Text.Encoding.UTF8)) {
            program.VertexShader = reader.ReadToEnd();
        }
        using (var reader = new StreamReader(fragStream, System.Text.Encoding.UTF8)) {
            program.FragmentShader = reader.ReadToEnd();
        }

        context.Acquire<DefaultShaderProgram>(GLRendererLayer.DefaultShaderProgramId);
        Console.WriteLine("Default shader program loaded.");
    }
}
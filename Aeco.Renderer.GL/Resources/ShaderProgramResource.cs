namespace Aeco.Renderer.GL;

using System.Reflection;

public class ShaderProgramResource : IGLResource
{
    public static ShaderProgramResource Default {
        get {
            if (s_defaultProgram != null) {
                return s_defaultProgram;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var vertStream = assembly.GetManifestResourceStream("Aeco.Renderer.GL.Embeded.Shaders.vert.glsl");
            if (vertStream == null) {
                throw new FileNotFoundException("Failed to load default vertex shader");
            }
            var fragStream = assembly.GetManifestResourceStream("Aeco.Renderer.GL.Embeded.Shaders.frag.glsl");
            if (fragStream == null) {
                throw new FileNotFoundException("Failed to load default fragment shader");
            }

            string vertexShader;
            string fragmentShader;
            using (var reader = new StreamReader(vertStream, System.Text.Encoding.UTF8)) {
                vertexShader = reader.ReadToEnd();
            }
            using (var reader = new StreamReader(fragStream, System.Text.Encoding.UTF8)) {
                fragmentShader = reader.ReadToEnd();
            }
            s_defaultProgram = new ShaderProgramResource(vertexShader, fragmentShader);
            return s_defaultProgram;
        }
    }

    private static ShaderProgramResource? s_defaultProgram;

    public string VertexShader;
    public string FragmentShader;

    public ShaderProgramResource(string vertexShader, string fragmentShader)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
    }
}
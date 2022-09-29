namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class ShaderProgramInitializer : VirtualLayer, IGLUpdateLayer
{
    private Query<Modified<ShaderProgram>, ShaderProgram> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var shaderProgram = ref context.Inspect<ShaderProgram>(id);

            // specify sources

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShader, shaderProgram.VertexShader);
            GL.ShaderSource(fragmentShader, shaderProgram.FragmentShader);

            // compile vertex shader

            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);

            if (success == 0) {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                Console.WriteLine(infoLog);
            }

            // compile fragment shader

            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);

            if (success == 0) {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                Console.WriteLine(infoLog);
            }

            // create program handle

            int program = GL.CreateProgram();

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0) {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine(infoLog);
            }

            // detach shaders

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // create handles component

            ref var handles = ref context.Acquire<ShaderProgramHandle>(id);
            handles.Value = program;
            handles.UniformLocations = new UniformLocations {
                World = GL.GetUniformLocation(program, "World"),
                View = GL.GetUniformLocation(program, "View"),
                Projection = GL.GetUniformLocation(program, "Projection")
            };
            Console.WriteLine($"Shader program {id} initialized.");
        }
    }
}
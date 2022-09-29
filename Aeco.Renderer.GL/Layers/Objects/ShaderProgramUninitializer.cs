namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class ShaderProgramUninitializer : UninitializerBase<ShaderProgram, ShaderProgramHandle>
{
    protected override void Uninitialize(in ShaderProgramHandle handle)
    {
        GL.DeleteProgram(handle.Value);
    }
}
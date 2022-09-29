namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class MeshUninitializer : UninitializerBase<Mesh, MeshHandles>
{
    protected override void Uninitialize(in MeshHandles handles)
    {
        GL.DeleteBuffer(handles.VertexBuffer);
        GL.DeleteVertexArray(handles.VertexArray);
        if (handles.ElementBuffer != 0) {
            GL.DeleteBuffer(handles.ElementBuffer);
        }
    }
}
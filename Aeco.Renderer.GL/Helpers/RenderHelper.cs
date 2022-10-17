namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public static class RenderHelper
{
    public static void SetInstancingMatrices()
    {
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 0);
        GL.VertexAttribDivisor(4, 1);
        GL.EnableVertexAttribArray(5);
        GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 4 * sizeof(float));
        GL.VertexAttribDivisor(5, 1);
        GL.EnableVertexAttribArray(6);
        GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 2 * 4 * sizeof(float));
        GL.VertexAttribDivisor(6, 1);
        GL.EnableVertexAttribArray(7);
        GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 3 * 4 * sizeof(float));
        GL.VertexAttribDivisor(7, 1);

        GL.EnableVertexAttribArray(8);
        GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 4 * 4 * sizeof(float));
        GL.VertexAttribDivisor(8, 1);
        GL.EnableVertexAttribArray(9);
        GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 5 * 4 * sizeof(float));
        GL.VertexAttribDivisor(9, 1);
        GL.EnableVertexAttribArray(10);
        GL.VertexAttribPointer(10, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 6 * 4 * sizeof(float));
        GL.VertexAttribDivisor(10, 1);
        GL.EnableVertexAttribArray(11);
        GL.VertexAttribPointer(11, 4, VertexAttribPointerType.Float, false, MeshInstance.MemorySize, 7 * 4 * sizeof(float));
        GL.VertexAttribDivisor(11, 1);
    }
}
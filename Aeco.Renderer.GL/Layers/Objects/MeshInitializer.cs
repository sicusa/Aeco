namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshInitializer : VirtualLayer, IGLUpdateLayer
{
    private Query<Modified<Mesh>, Mesh> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref readonly var mesh = ref context.Inspect<Mesh>(id);
            ref var handles = ref context.Acquire<MeshHandles>(id, out bool exists);

            if (!exists) {
                handles.VertexArray = GL.GenVertexArray();
                handles.VertexBuffer = GL.GenBuffer();
            }
            
            GL.BindVertexArray(handles.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handles.VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsageHint.StaticDraw);

            if (mesh.Indeces != null) {
                if (!exists) {
                    handles.ElementBuffer = GL.GenBuffer();
                }
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, handles.ElementBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Indeces.Length * sizeof(uint), mesh.Indeces, BufferUsageHint.StaticDraw);
            }

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            Console.WriteLine($"Mesh {id} initialized.");
        }
    }
}
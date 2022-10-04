namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderer : VirtualLayer, IGLLoadLayer, IGLRenderLayer
{
    private Group<MeshRenderable> _g = new();

    public void OnLoad(IDataLayer<IComponent> context)
        => _g.Refrash(context);

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        var cameraId = context.Singleton<Camera>();
        ref var cameraProj = ref context.Acquire<CameraMatrices>(cameraId).ProjectionRaw;
        ref var cameraView = ref context.UnsafeAcquire<WorldView>(cameraId).ViewRaw;
        UniformLocations uniforms;

        foreach (var id in _g.Query(context)) {
            ref readonly var renderable = ref context.Inspect<MeshRenderable>(id);
            try {
                var materials = renderable.Materials;
                if (materials != null && materials.Length != 0) {
                    ref readonly var material = ref context.Inspect<Material>(materials[0]);
                    ref readonly var shaderProgramHandle = ref context.Inspect<ShaderProgramHandle>(material.ShaderProgram);

                    GL.UseProgram(shaderProgramHandle.Value);
                    if (context.TryGet<TextureHandle>(material.Texture, out var textureHandle)) {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, textureHandle.Value);
                    }
                    uniforms = shaderProgramHandle.UniformLocations;
                }
                else {
                    ref readonly var shaderProgramHandle = ref context.Inspect<ShaderProgramHandle>(
                        GLRenderer.DefaultShaderProgramId);
                    GL.UseProgram(shaderProgramHandle.Value);
                    uniforms = shaderProgramHandle.UniformLocations;
                }

                ref var matrices = ref context.Acquire<TransformMatrices>(id);
                GL.UniformMatrix4(uniforms.World, 1, true, ref matrices.WorldRaw.M11);
                GL.UniformMatrix4(uniforms.View, 1, true, ref cameraView.M11);
                GL.UniformMatrix4(uniforms.Projection, 1, true, ref cameraProj.M11);

                ref readonly var mesh = ref context.Inspect<Mesh>(renderable.Mesh);
                ref readonly var handles = ref context.Inspect<MeshHandles>(renderable.Mesh);

                GL.BindVertexArray(handles.VertexArray);
                if (mesh.Indeces != null) {
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indeces.Length, DrawElementsType.UnsignedInt, 0);
                }
                else {
                    GL.DrawArrays(PrimitiveType.Triangles, 0, mesh.Vertices.Length / 5);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Failed to render {id}: " + e);
            }
        }
    }
}
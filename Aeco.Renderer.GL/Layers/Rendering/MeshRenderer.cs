namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderer : VirtualLayer, IGLLoadLayer, IGLRenderLayer
{
    private Group<Renderable, MeshData> _g = new();

    public void OnLoad(IDataLayer<IComponent> context)
        => _g.Refrash(context);

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        if (context.TryGet<TextureData>(GLRenderer.DefaultTextureId, out var data)) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, data.Handle);
        }
        var cameraUniformHandle = context.InspectAny<CameraUniformBufferHandle>().Value;
        int mainLightHandle = context.AcquireAny<MainLightUniformBufferHandle>().Value;

        foreach (var id in _g.Query(context)) {
            try {
                ref readonly var meshData = ref context.Inspect<MeshData>(id);
                ref readonly var materialData = ref context.Inspect<MaterialData>(meshData.MaterialId);
                ref readonly var shaderProgramData = ref context.Inspect<ShaderProgramData>(materialData.ShaderProgramId);

                GL.UseProgram(shaderProgramData.Handle);

                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, cameraUniformHandle);
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 2,
                    context.Require<ObjectUniformBufferHandle>(id).Value);
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 3, materialData.Handle);
                
                if (mainLightHandle != -1) {
                    GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 4, mainLightHandle);
                }

                // bind textures

                var textureLocations = shaderProgramData.UniformLocations.Textures;
                var textures = materialData.Textures;
                for (int i = 0; i != textures.Count; ++i) {
                    int location = textureLocations[i];
                    if (location == -1) { continue; };
                    var texId = textures[i];
                    if (texId == null) { continue; }
                    var textureData = context.Inspect<TextureData>(texId.Value);
                    GL.ActiveTexture(TextureUnit.Texture1 + i);
                    GL.BindTexture(TextureTarget.Texture2D, textureData.Handle);
                    GL.Uniform1(location, i + 1);
                }

                // draw

                GL.BindVertexArray(meshData.VertexArrayHandle);
                GL.DrawElements(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
            catch (Exception e) {
                Console.WriteLine($"Failed to render {id}: " + e);
            }
        }
    }
}
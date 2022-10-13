namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderer : VirtualLayer, IGLLoadLayer, IGLUpdateLayer, IGLRenderLayer
{
    private Group<Renderable, MeshData> _g = new();

    public void OnLoad(IDataLayer<IComponent> context)
        => _g.Refrash(context);

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
        => _g.Query(context);

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        var cameraUniformHandle = context.InspectAny<CameraUniformBufferHandle>().Value;
        int mainLightHandle = context.AcquireAny<MainLightUniformBufferHandle>().Value;
        var defaultTexData = context.Inspect<TextureData>(GLRenderer.DefaultTextureId);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, defaultTexData.Handle);

        foreach (var id in _g) {
            try {
                ref readonly var mesh = ref context.Inspect<Mesh>(id);
                ref readonly var meshData = ref context.Inspect<MeshData>(id);
                ref readonly var material = ref context.Inspect<Material>(meshData.MaterialId);
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
                var meshResource = mesh.Resource;
                if (meshResource.Indeces != null) {
                    GL.DrawElements(PrimitiveType.Triangles, meshResource.Indeces!.Length, DrawElementsType.UnsignedInt, 0);
                }
                else {
                    GL.DrawArrays(PrimitiveType.Triangles, 0, meshResource.Vertices!.Length);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Failed to render {id}: " + e);
            }
        }
    }
}
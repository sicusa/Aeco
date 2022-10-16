namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class MeshRenderer : VirtualLayer, IGLRenderLayer
{
    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        if (context.TryGet<TextureData>(GLRenderer.DefaultTextureId, out var data)) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, data.Handle);
        }

        var cameraUniformHandle = context.InspectAny<CameraUniformBufferHandle>().Value;
        int mainLightHandle = context.AcquireAny<MainLightUniformBufferHandle>().Value;

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, cameraUniformHandle);
        if (mainLightHandle != -1) {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 4, mainLightHandle);
        }

        foreach (var id in context.Query<MeshRendering>()) {
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            ref readonly var materialData = ref context.Inspect<MaterialData>(meshData.MaterialId);

            GL.BindVertexArray(meshData.VertexArrayHandle);
            ApplyMaterial(context, in materialData);

            var referencers = context.Require<ResourceReferencers>(id).Ids;
            foreach (var referencerId in referencers) {
                if (!context.Contains<MeshRenderable>(referencerId)) {
                    continue;
                }
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 2,
                    context.Require<ObjectUniformBufferHandle>(referencerId).Value);
                if (context.TryGet<MaterialData>(referencerId, out var overwritingMaterialData)) {
                    ApplyMaterial(context, in overwritingMaterialData);
                }
                GL.DrawElements(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
        }
        
    }

    private void ApplyMaterial(IDataLayer<IComponent> context, in MaterialData materialData)
    {
        ref readonly var shaderProgramData = ref context.Inspect<ShaderProgramData>(materialData.ShaderProgramId);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 3, materialData.Handle);
        GL.UseProgram(shaderProgramData.Handle);

        var textures = materialData.Textures;
        var textureLocations = shaderProgramData.UniformLocations.Textures;

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
    }
}
namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class MeshRenderer : VirtualLayer, IGLRenderLayer
{
    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        if (context.TryGet<TextureData>(GLRenderer.DefaultTextureId, out var textureData)) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureData.Handle);
        }
        bool vertexArrayBound = false;
        foreach (var id in context.Query<Mesh>()) {
            if (!context.TryGet<MeshRenderingState>(id, out var state)) {
                continue;
            }
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            ref readonly var materialData = ref context.Inspect<MaterialData>(meshData.MaterialId);

            GL.BindVertexArray(meshData.VertexArrayHandle);
            vertexArrayBound = true;
            ApplyMaterial(context, in materialData);

            int instanceCount = state.Instances.Count;
            if (instanceCount == 1) {
                GL.DrawElements(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            else if (instanceCount != 0) {
                GL.DrawElementsInstanced(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);
            }

            foreach (var variantId in state.VariantIds) {
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 2,
                    context.Require<VariantUniformBuffer>(variantId).Handle);
                if (context.TryGet<MaterialData>(variantId, out var overwritingMaterialData)) {
                    ApplyMaterial(context, in overwritingMaterialData);
                }
                GL.DrawElements(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
        }
        if (vertexArrayBound) {
            GL.BindVertexArray(0);
        }
    }

    private void ApplyMaterial(IDataLayer<IComponent> context, in MaterialData materialData)
    {
        ref readonly var shaderProgramData = ref context.Inspect<ShaderProgramData>(materialData.ShaderProgramId);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Material, materialData.Handle);
        GL.UseProgram(shaderProgramData.Handle);

        var textures = materialData.Textures;
        var textureLocations = shaderProgramData.UniformLocations.Textures;

        for (int i = 0; i != textures.Length; ++i) {
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
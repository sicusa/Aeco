namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderer : VirtualLayer, IGLLoadLayer, IGLRenderLayer
{
    private Group<Mesh> _g = new();

    public void OnLoad(IDataLayer<IComponent> context)
        => _g.Refresh(context);

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        _g.Query(context);

        var cullProgram = context.Inspect<ShaderProgramData>(GLRenderer.CullingShaderProgramId).Handle;
        GL.UseProgram(cullProgram);
        GL.Enable(EnableCap.RasterizerDiscard);

        foreach (var id in _g) {
            if (!context.TryGet<MeshRenderingState>(id, out var state)) {
                continue;
            }
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            ref readonly var meshUniformBuffer = ref context.Inspect<MeshUniformBuffer>(id);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Mesh, meshUniformBuffer.Handle);
            GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, 0, meshData.BufferHandles[MeshBufferType.CulledInstance]);
            GL.BindVertexArray(meshData.CullingVertexArrayHandle);

            GL.BeginTransformFeedback(TransformFeedbackPrimitiveType.Points);
            GL.BeginQuery(QueryTarget.PrimitivesGenerated, meshData.CulledQueryHandle);
            GL.DrawArrays(PrimitiveType.Points, 0, state.Instances.Count);
            GL.EndQuery(QueryTarget.PrimitivesGenerated);
            GL.EndTransformFeedback();
        }

        GL.Disable(EnableCap.RasterizerDiscard);

        if (context.TryGet<TextureData>(GLRenderer.DefaultTextureId, out var textureData)) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureData.Handle);
        }

        bool vertexArrayBound = false;
        foreach (var id in _g) {
            if (!context.TryGet<MeshRenderingState>(id, out var state)) {
                continue;
            }
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            ref readonly var materialData = ref context.Inspect<MaterialData>(meshData.MaterialId);

            GL.BindVertexArray(meshData.VertexArrayHandle);
            ApplyMaterial(context, in materialData);
            vertexArrayBound = true;

            GL.GetQueryObject(meshData.CulledQueryHandle, GetQueryObjectParam.QueryResult, out int instanceCount);
            if (instanceCount > 0) {
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

        var errorCode = GL.GetError();
        if (errorCode != ErrorCode.NoError) {
            Console.WriteLine("Warning: OpenGL error code " + errorCode);
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
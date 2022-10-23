namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class MeshRenderer : VirtualLayer, IGLLoadLayer, IGLResizeLayer, IGLRenderLayer
{
    private Group<Mesh> _g = new();
    private int _windowWidth;
    private int _windowHeight;
    private int _defaultVertexArray;

    public void OnLoad(IDataLayer<IComponent> context)
    {
        _g.Refresh(context);
        _defaultVertexArray = GL.GenVertexArray();
    }

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        bool vertexArrayBound = false;

        ref var framebuffer = ref context.Require<FramebufferData>(GLRenderer.DefaultFramebufferId);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer.Handle);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Framebuffer, framebuffer.UniformBufferHandle);
        GL.BindVertexArray(_defaultVertexArray);

        // hierarchical-Z occlusion culling

        ref readonly var hizProgramData = ref context.Inspect<ShaderProgramData>(GLRenderer.HierarchicalZShaderProgramId);
        int lastMipSizeLocation = hizProgramData.CustomLocations["LastMipSize"];

        GL.UseProgram(hizProgramData.Handle);
        GL.ColorMask(true, false, false, false);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.MaxDepthTextureHandle);
        GL.Uniform1(hizProgramData.CustomLocations["LastMaxMip"], 0);

        GL.DepthFunc(DepthFunction.Always);

        int width = framebuffer.Width;
        int height = framebuffer.Height;
        int levelCount = 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(width, height)));
        
        for (int i = 1; i < levelCount; ++i) {
            GL.Uniform2(lastMipSizeLocation, width, height);

            width /= 2;
            height /= 2;
            width = width > 0 ? width : 1;
            height = height > 0 ? height : 1;
            GL.Viewport(0, 0, width, height);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, i - 1);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, i - 1);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, framebuffer.MaxDepthTextureHandle, i);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, levelCount - 1);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebuffer.ColorTextureHandle, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, framebuffer.MaxDepthTextureHandle, 0);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.ColorMask(true, true, true, true);
        GL.Viewport(0, 0, framebuffer.Width, framebuffer.Height);

        // instance cloud culling

        var cullProgram = context.Inspect<ShaderProgramData>(GLRenderer.CullingShaderProgramId);
        GL.UseProgram(cullProgram.Handle);
        GL.Enable(EnableCap.RasterizerDiscard);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.MaxDepthTextureHandle);

        foreach (var id in _g.Query(context)) {
            if (!context.TryGet<MeshRenderingState>(id, out var state)) {
                continue;
            }
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            ref readonly var meshUniformBuffer = ref context.Inspect<MeshUniformBuffer>(id);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Mesh, meshUniformBuffer.Handle);
            GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, 0, meshData.BufferHandles[MeshBufferType.CulledInstance]);
            GL.BindVertexArray(meshData.CullingVertexArrayHandle);
            vertexArrayBound = true;

            GL.BeginTransformFeedback(TransformFeedbackPrimitiveType.Points);
            GL.BeginQuery(QueryTarget.PrimitivesGenerated, meshData.CulledQueryHandle);
            GL.DrawArrays(PrimitiveType.Points, 0, state.Instances.Count);
            GL.EndQuery(QueryTarget.PrimitivesGenerated);
            GL.EndTransformFeedback();
        }

        GL.Disable(EnableCap.RasterizerDiscard);

        // rendering

        if (context.TryGet<TextureData>(GLRenderer.DefaultTextureId, out var textureData)) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureData.Handle);
        }

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
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Object,
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

        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        GL.BindVertexArray(_defaultVertexArray);

        ref readonly var postProgram = ref context.Inspect<ShaderProgramData>(GLRenderer.PostProcessingShaderProgramId);
        var customLocations = postProgram.CustomLocations;
        GL.UseProgram(postProgram.Handle);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.ColorTextureHandle);
        GL.Uniform1(customLocations["ColorBuffer"], 0);

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.MaxDepthTextureHandle);
        GL.Uniform1(customLocations["MaxDepthBuffer"], 1);

        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.MinDepthTextureHandle);
        GL.Uniform1(customLocations["MinDepthBuffer"], 2);

        GL.Disable(EnableCap.DepthTest);
        GL.DrawArrays(PrimitiveType.Points, 0, 1);
        GL.Enable(EnableCap.DepthTest);
    }

    public void OnResize(IDataLayer<IComponent> context, int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    private void ApplyMaterial(IDataLayer<IComponent> context, in MaterialData materialData)
    {
        ref readonly var shaderProgramData = ref context.Inspect<ShaderProgramData>(materialData.ShaderProgramId);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Material, materialData.Handle);
        GL.UseProgram(shaderProgramData.Handle);

        var textures = materialData.Textures;
        var textureLocations = shaderProgramData.TextureLocations;

        for (int i = 0; i != textures.Length; ++i) {
            int location = textureLocations![i];
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
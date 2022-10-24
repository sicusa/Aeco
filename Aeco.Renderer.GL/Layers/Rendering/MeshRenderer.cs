namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;
using System.Collections.Generic;

public class MeshRenderer : VirtualLayer, IGLLoadLayer, IGLResizeLayer, IGLRenderLayer
{
    private Group<Mesh, MeshRenderingState> _g = new();
    private List<Guid> _transparentIds = new();
    private int _windowWidth;
    private int _windowHeight;
    private int _defaultVertexArray;

    private DrawBuffersEnum[] _normalDraw = { DrawBuffersEnum.ColorAttachment0 };
    private DrawBuffersEnum[] _transparentDraw = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };

    public void OnLoad(IDataLayer<IComponent> context)
    {
        _g.Refresh(context);
        _defaultVertexArray = GL.GenVertexArray();
    }

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        ref readonly var framebuffer = ref context.Inspect<FramebufferData>(GLRenderer.DefaultFramebufferId);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer.Handle);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Framebuffer, framebuffer.UniformBufferHandle);
        GL.BindVertexArray(_defaultVertexArray);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.DepthTextureHandle);

        // motion-based depth filling

        ref readonly var motionProgramData = ref context.Inspect<ShaderProgramData>(GLRenderer.MotionShaderProgramId);

        GL.UseProgram(motionProgramData.Handle);
        GL.ColorMask(false, false, false, false);
        GL.DepthFunc(DepthFunction.Always);
        GL.DrawArrays(PrimitiveType.Points, 0, 1);

        // hierarchical-Z occlusion culling

        ref readonly var hizProgramData = ref context.Inspect<ShaderProgramData>(GLRenderer.HierarchicalZShaderProgramId);
        int lastMipSizeLocation = hizProgramData.CustomLocations["LastMipSize"];

        GL.UseProgram(hizProgramData.Handle);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.DepthTextureHandle);

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
                FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, framebuffer.DepthTextureHandle, i);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, levelCount - 1);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebuffer.ColorTextureHandle, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, framebuffer.DepthTextureHandle, 0);

        GL.DepthFunc(DepthFunction.Lequal);
        GL.ColorMask(true, true, true, true);
        GL.Viewport(0, 0, framebuffer.Width, framebuffer.Height);

        // instance cloud culling

        var cullProgram = context.Inspect<ShaderProgramData>(GLRenderer.CullingShaderProgramId);
        GL.UseProgram(cullProgram.Handle);
        GL.Enable(EnableCap.RasterizerDiscard);

        foreach (var id in _g.Query(context)) {
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            Cull(context, id, in meshData, in framebuffer);
        }

        GL.Disable(EnableCap.RasterizerDiscard);

        // render opaque meshes

        ref readonly var defaultTexData = ref context.Inspect<TextureData>(GLRenderer.DefaultTextureId);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, defaultTexData.Handle);
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

        foreach (var id in _g) {
            ref readonly var meshData = ref context.Inspect<MeshData>(id);
            if (meshData.IsTransparent) {
                _transparentIds.Add(id);
                continue;
            }
            Render(context, id, in meshData, in framebuffer);
        }

        // render transparent objects
        if (_transparentIds.Count != 0) {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebuffer.TransparencyAccumTextureHandle, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, framebuffer.TransparencyAlphaTextureHandle, 0);
            GL.DrawBuffers(2, _transparentDraw);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.DepthMask(false);
            GL.Enable(EnableCap.Blend);
            GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcAlpha);

            foreach (var id in _transparentIds) {
                ref readonly var meshData = ref context.Inspect<MeshData>(id);
                Render(context, id, in meshData, in framebuffer);
            }
            
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebuffer.ColorTextureHandle, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, 0, 0);
            GL.DrawBuffers(1, _normalDraw);

            // compose transparency

            ref readonly var composeProgram = ref context.Inspect<ShaderProgramData>(GLRenderer.TransparencyComposeShaderProgramId);

            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            GL.UseProgram(composeProgram.Handle);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, framebuffer.TransparencyAccumTextureHandle);
            GL.Uniform1(composeProgram.CustomLocations["AccumColorTex"], 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, framebuffer.TransparencyAlphaTextureHandle);
            GL.Uniform1(composeProgram.CustomLocations["AccumAlphaTex"], 1);

            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            _transparentIds.Clear();
        }

        // render post-processed result

        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        GL.BindVertexArray(_defaultVertexArray);

        ref readonly var postProgram = ref context.Inspect<ShaderProgramData>(GLRenderer.PostProcessingShaderProgramId);
        var customLocations = postProgram.CustomLocations;
        GL.UseProgram(postProgram.Handle);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.ColorTextureHandle);
        GL.Uniform1(customLocations["ColorBuffer"], 0);

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, framebuffer.DepthTextureHandle);
        GL.Uniform1(postProgram.DepthBufferLocation, 1);

        GL.Disable(EnableCap.DepthTest);
        GL.DrawArrays(PrimitiveType.Points, 0, 1);
        GL.Enable(EnableCap.DepthTest);
    }

    private void Cull(IDataLayer<IComponent> context, Guid id, in MeshData meshData, in FramebufferData framebuffer)
    {
        ref readonly var meshUniformBuffer = ref context.Inspect<MeshUniformBuffer>(id);
        ref readonly var state = ref context.Inspect<MeshRenderingState>(id);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Mesh, meshUniformBuffer.Handle);
        GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, 0, meshData.BufferHandles[MeshBufferType.CulledInstance]);
        GL.BindVertexArray(meshData.CullingVertexArrayHandle);

        GL.BeginTransformFeedback(TransformFeedbackPrimitiveType.Points);
        GL.BeginQuery(QueryTarget.PrimitivesGenerated, meshData.CulledQueryHandle);
        GL.DrawArrays(PrimitiveType.Points, 0, state.Instances.Count);
        GL.EndQuery(QueryTarget.PrimitivesGenerated);
        GL.EndTransformFeedback();
    }

    private void Render(IDataLayer<IComponent> context, Guid id, in MeshData meshData, in FramebufferData framebuffer)
    {
        ref readonly var materialData = ref context.Inspect<MaterialData>(meshData.MaterialId);
        ref readonly var state = ref context.Inspect<MeshRenderingState>(id);

        GL.BindVertexArray(meshData.VertexArrayHandle);
        ApplyMaterial(context, in materialData, in framebuffer);

        GL.GetQueryObject(meshData.CulledQueryHandle, GetQueryObjectParam.QueryResult, out int instanceCount);
        if (instanceCount > 0) {
            GL.DrawElementsInstanced(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);
        }

        foreach (var variantId in state.VariantIds) {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Object,
                context.Require<VariantUniformBuffer>(variantId).Handle);
            if (context.TryGet<MaterialData>(variantId, out var overwritingMaterialData)) {
                ApplyMaterial(context, in overwritingMaterialData, in framebuffer);
            }
            GL.DrawElements(PrimitiveType.Triangles, meshData.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

    public void OnResize(IDataLayer<IComponent> context, int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    private void ApplyMaterial(IDataLayer<IComponent> context, in MaterialData materialData, in FramebufferData framebufferData)
    {
        ref readonly var shaderProgramData = ref context.Inspect<ShaderProgramData>(materialData.ShaderProgramId);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Material, materialData.Handle);
        GL.UseProgram(shaderProgramData.Handle);

        var textures = materialData.Textures;
        var texturesLength = textures.Length;
        var textureLocations = shaderProgramData.TextureLocations;

        for (int i = 0; i != texturesLength; ++i) {
            int location = textureLocations![i];
            if (location == -1) { continue; };
            var texId = textures[i];
            if (texId == null) { continue; }
            var textureData = context.Inspect<TextureData>(texId.Value);
            GL.ActiveTexture(TextureUnit.Texture1 + i);
            GL.BindTexture(TextureTarget.Texture2D, textureData.Handle);
            GL.Uniform1(location, i + 1);
        }

        if (shaderProgramData.DepthBufferLocation != -1) {
            GL.ActiveTexture(TextureUnit.Texture1 + texturesLength + 1);
            GL.BindTexture(TextureTarget.Texture2D, framebufferData.DepthTextureHandle);
            GL.Uniform1(texturesLength, texturesLength + 2);
        }
    }
}
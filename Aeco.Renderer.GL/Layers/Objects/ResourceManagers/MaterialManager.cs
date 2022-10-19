namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class MaterialManager : ResourceManagerBase<Material, MaterialData, MaterialResource>
{
    protected override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref Material material, ref MaterialData data, bool updating)
    {
        if (updating) {
            Uninitialize(context, id, in material, in data);
        }

        data.Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, data.Handle);
        GL.BufferData(BufferTarget.UniformBuffer, 16 * 4 + 8 * 3, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        data.ShaderProgramId =
            material.ShaderProgram != null
                ? ResourceLibrary<ShaderProgramResource>.Reference<ShaderProgram>(context, material.ShaderProgram, id)
                : GLRenderer.DefaultShaderProgramId;

        var resource = material.Resource;
        var textureReferences = new EnumArray<TextureType, Guid?>();
        var textures = resource.Textures;

        for (int i = 0; i < textures.Length; ++i) {
            var texResource = textures[i];
            if (texResource != null) {
                textureReferences[i] = ResourceLibrary<TextureResource>.Reference<Texture>(context, texResource, id);
            }
        }
        data.Textures = textureReferences;

        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 16, ref resource.DiffuseColor.X);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16, 16, ref resource.SpecularColor.X);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16 * 2, 16, ref resource.AmbientColor.X);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16 * 3, 16, ref resource.EmissiveColor.X);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16 * 4, 4, ref resource.Shininess);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16 * 4 + 8, 8, ref material.Tiling.X);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero + 16 * 4 + 8 * 2, 8, ref material.Offset.X);
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in Material material, in MaterialData data)
    {
        GL.DeleteBuffer(data.Handle);

        ResourceLibrary<ShaderProgramResource>.Unreference(context, data.ShaderProgramId, id);

        var textures = data.Textures;
        if (textures != null) {
            for (int i = 0; i < (int)TextureType.Unknown; ++i) {
                var texId = textures[i];
                if (texId != null) {
                    ResourceLibrary<TextureResource>.Unreference(context, texId.Value, id);
                }
            }
        }
    }
}
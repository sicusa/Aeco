namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

public class MaterialManager : ResourceManagerBase<Material, MaterialData, MaterialResource>
{
    protected unsafe override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref Material material, ref MaterialData data, bool updating)
    {
        if (updating) {
            Uninitialize(context, id, in material, in data);
        }

        data.Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, data.Handle);
        data.Pointer = GLHelper.InitializeBuffer(BufferTarget.UniformBuffer, MaterialParameters.MemorySize);

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

        fixed (MaterialParameters* parameters = &resource.Parameters) {
            System.Buffer.MemoryCopy(&parameters->DiffuseColor, (void*)data.Pointer,
                MaterialParameters.MemorySize, MaterialParameters.MemorySize);
        }
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
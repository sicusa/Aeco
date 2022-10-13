namespace Aeco.Renderer.GL;

using System.Numerics;

using Assimp.Configs;

using OpenTK.Graphics.OpenGL4;

public static class ModelHelper
{
    private class AssimpLoaderState
    {
        public Dictionary<Assimp.Mesh, MeshResource> LoadedMeshes = new();
        public Dictionary<Assimp.Material, MaterialResource> LoadedMaterials = new();
        public Dictionary<string, TextureResource> LoadedTextures = new();
    }

    private static Assimp.AssimpContext? _assimpImporter;

    public static ModelResource Load(Stream stream, string? formatHint = null)
    {
        if (_assimpImporter == null) {
            _assimpImporter = new();
            _assimpImporter.SetConfig(new NormalSmoothingAngleConfig(66));
        }
        var scene = _assimpImporter.ImportFileFromStream(
            stream, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality, formatHint);
        Console.WriteLine(scene);
        return new ModelResource {
            RootNode = LoadNode(new AssimpLoaderState(), scene, scene.RootNode)
        };
    }

    private static ModelNodeResource LoadNode(AssimpLoaderState state, Assimp.Scene scene, Assimp.Node node)
    {
        var nodeResource = new ModelNodeResource {
            Name = node.Name
        };

        var metadata = nodeResource.Metadata;
        foreach (var (k, v) in node.Metadata) {
            metadata.Add(k,
                v.DataType == Assimp.MetaDataType.Vector3D
                    ? FromVector(v.DataAs<Assimp.Vector3D>()!.Value) : v);
        }

        Matrix4x4.Decompose(FromMatrix(node.Transform),
            out nodeResource.Scale,
            out nodeResource.Rotation,
            out nodeResource.Position);

        if (node.HasMeshes) {
            nodeResource.Meshes = node.MeshIndices
                .Select(index => LoadMesh(state, scene, scene.Meshes[index])).ToArray();
        }
        if (node.HasChildren) {
            nodeResource.Children = node.Children
                .Select(node => LoadNode(state, scene, node)).ToArray();
        }
        return nodeResource;
    }

    private static MeshResource LoadMesh(AssimpLoaderState state, Assimp.Scene scene, Assimp.Mesh mesh)
    {
        if (state.LoadedMeshes.TryGetValue(mesh, out var meshResource)) {
            return meshResource;
        }

        meshResource = new MeshResource {
            Vertices = mesh.Vertices.Select(FromVector).ToArray(),
            TexCoords = mesh.TextureCoordinateChannels[0].Select(FromVector).ToArray(),
            Normals = mesh.Normals.Select(FromVector).ToArray(),
            Tangents = mesh.Tangents.Select(FromVector).ToArray(),
            Indeces = mesh.GetIndices(),
            Material = LoadMaterial(state, scene, scene.Materials[mesh.MaterialIndex])
        };

        ref var min = ref meshResource.BoudingBox.Min;
        ref var max = ref meshResource.BoudingBox.Max;

        for (int i = 0; i < mesh.VertexCount; ++i) {
            var vertex = FromVector(mesh.Vertices[i]);

            min.X = MathF.Min(min.X, vertex.X);
            min.Y = MathF.Min(min.Y, vertex.Y);
            min.Z = MathF.Min(min.Z, vertex.Z);

            max.X = MathF.Max(max.X, vertex.X);
            max.Y = MathF.Max(max.Y, vertex.Y);
            max.Z = MathF.Max(max.Z, vertex.Z);
        }

        state.LoadedMeshes[mesh] = meshResource;
        return meshResource;
    }

    private static MaterialResource LoadMaterial(AssimpLoaderState state, Assimp.Scene scene, Assimp.Material mat)
    {
        if (state.LoadedMaterials.TryGetValue(mat, out var materialResource)) {
            return materialResource;
        }

        materialResource = new MaterialResource {
            DiffuseColor = mat.HasColorDiffuse ? FromColor(mat.ColorDiffuse) : Vector4.One,
            SpecularColor = mat.HasColorSpecular ? FromColor(mat.ColorSpecular) : Vector4.Zero,
            AmbientColor = mat.HasColorAmbient ? FromColor(mat.ColorAmbient) : new Vector4(0.2f, 0.2f, 0.2f, 1f),
            EmissiveColor = mat.HasColorEmissive ? FromColor(mat.ColorEmissive) : Vector4.Zero,
            Shininess = mat.HasShininess ? mat.Shininess : 0,
            ShininessStrength = mat.HasShininessStrength ? mat.ShininessStrength : 1f,
            Opacity = mat.HasOpacity ? mat.Opacity : 1f
        };

        var textures = materialResource.Textures;

        if (mat.HasTextureDiffuse) { textures[TextureType.Diffuse] = LoadTexture(state, scene, mat.TextureDiffuse); }
        if (mat.HasTextureSpecular) { textures[TextureType.Specular] = LoadTexture(state, scene, mat.TextureSpecular); }
        if (mat.HasTextureAmbient) { textures[TextureType.Ambient] = LoadTexture(state, scene, mat.TextureAmbient); }
        if (mat.HasTextureEmissive) { textures[TextureType.Emissive] = LoadTexture(state, scene, mat.TextureEmissive); }
        if (mat.HasTextureHeight) { textures[TextureType.Height] = LoadTexture(state, scene, mat.TextureHeight); }
        if (mat.HasTextureHeight) { textures[TextureType.Height] = LoadTexture(state, scene, mat.TextureHeight); }
        if (mat.HasTextureNormal) { textures[TextureType.Normal] = LoadTexture(state, scene, mat.TextureNormal); }
        if (mat.HasTextureOpacity) { textures[TextureType.Opacity] = LoadTexture(state, scene, mat.TextureOpacity); }
        if (mat.HasTextureDisplacement) { textures[TextureType.Displacement] = LoadTexture(state, scene, mat.TextureDisplacement); }
        if (mat.HasTextureLightMap) { textures[TextureType.Lightmap] = LoadTexture(state, scene, mat.TextureLightMap); }
        if (mat.HasTextureReflection) { textures[TextureType.Reflection] = LoadTexture(state, scene, mat.TextureReflection); }

        state.LoadedMaterials[mat] = materialResource;
        return materialResource;
    }

    private static TextureResource LoadTexture(AssimpLoaderState state, Assimp.Scene scene, Assimp.TextureSlot tex)
    {
        if (state.LoadedTextures.TryGetValue(tex.FilePath, out var textureResource)) {
            return textureResource;
        }
        try {
            textureResource = new TextureResource(LoadImage(state, scene, tex.FilePath)) {
                TextureType = FromTextureType(tex.TextureType),
                WrapU = FromTextureWrapMode(tex.WrapModeU),
                WrapV = FromTextureWrapMode(tex.WrapModeV)
            };
            state.LoadedTextures[tex.FilePath] = textureResource;
            return textureResource;
        }
        catch (Exception e) {
            Console.WriteLine("Failed to load texture: " + e);
            return TextureResource.None;
        }
    }

    private static ImageResource LoadImage(AssimpLoaderState state, Assimp.Scene scene, string filePath)
    {
        var embeddedTexture = scene.GetEmbeddedTexture(filePath);
        if (embeddedTexture == null) {
            return ImageHelper.LoadFromFile(filePath);
        }
        if (embeddedTexture.HasCompressedData) {
            return ImageHelper.Load(embeddedTexture.CompressedData);
        }
        var data = embeddedTexture.NonCompressedData;
        var bytes = new Byte[data.Length * 4];
        for (int i = 0; i < data.Length; ++i) {
            ref var texel = ref data[i];
            bytes[i * 4] = texel.R;
            bytes[i * 4 + 1] = texel.G;
            bytes[i * 4 + 2] = texel.B;
            bytes[i * 4 + 3] = texel.A;
        }
        return new ImageResource {
            Width = embeddedTexture.Width,
            Height = embeddedTexture.Height,
            Bytes = bytes
        };
    }

    private static Matrix4x4 FromMatrix(Assimp.Matrix4x4 mat)
        => new Matrix4x4(
            mat.A1, mat.A2, mat.A3, mat.A4,
            mat.B1, mat.B2, mat.B3, mat.B4,
            mat.C1, mat.C2, mat.C3, mat.C4,
            mat.D1, mat.D2, mat.D3, mat.D4);
    
    private static Vector3 FromVector(Assimp.Vector3D v)
        => new Vector3(v.X, v.Y, v.Z);

    private static Vector4 FromColor(Assimp.Color4D c)
        => new Vector4(c.R, c.G, c.B, c.A);
    
    private static TextureType FromTextureType(Assimp.TextureType type)
        => type switch {
            Assimp.TextureType.Ambient => TextureType.Ambient,
            Assimp.TextureType.Diffuse => TextureType.Diffuse,
            Assimp.TextureType.Displacement => TextureType.Displacement,
            Assimp.TextureType.Emissive => TextureType.Emissive,
            Assimp.TextureType.Height => TextureType.Height,
            Assimp.TextureType.Lightmap => TextureType.Lightmap,
            Assimp.TextureType.Normals => TextureType.Normal,
            Assimp.TextureType.Opacity => TextureType.Opacity,
            Assimp.TextureType.Reflection => TextureType.Reflection,
            Assimp.TextureType.Shininess => TextureType.Shininess,
            Assimp.TextureType.Specular => TextureType.Specular,
            _ => TextureType.Unknown
        };
    
    private static TextureWrapMode FromTextureWrapMode(Assimp.TextureWrapMode mode)
        => mode switch {
            Assimp.TextureWrapMode.Clamp => TextureWrapMode.ClampToEdge,
            Assimp.TextureWrapMode.Decal => TextureWrapMode.ClampToBorder,
            Assimp.TextureWrapMode.Mirror => TextureWrapMode.MirroredRepeat,
            Assimp.TextureWrapMode.Wrap => TextureWrapMode.Repeat,
            _ => TextureWrapMode.Repeat
        };
}
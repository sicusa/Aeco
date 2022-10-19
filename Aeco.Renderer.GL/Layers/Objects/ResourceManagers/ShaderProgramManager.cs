namespace Aeco.Renderer.GL;

using System.Text.RegularExpressions;

using OpenTK.Graphics.OpenGL4;

using System;

public class ShaderProgramManager : ResourceManagerBase<ShaderProgram, ShaderProgramData, ShaderProgramResource>
{
    private Dictionary<string, string> _internalShaderFiles = new() {
        ["nagule/common.glsl"] = 
@"#ifndef NAGULE_COMMON
#define NAGULE_COMMON

layout(std140) uniform Camera {
    mat4 Matrix_V;
    mat4 Matrix_P;
    mat4 Matrix_VP;
    vec3 CameraPosition;
};

layout(std140) uniform MainLight {
    vec3 MainLightDirection;
    vec4 MainLightColor;
};

layout(std140) uniform Material {
    vec4 Diffuse;
    vec4 Specular;
    vec4 Ambient;
    vec4 Emission;
    float Shininess;
    vec2 Tiling;
    vec2 Offset;
};

layout(std140) uniform Mesh {
    vec3 BoundingBoxMin;
    vec3 BoundingBoxMax;
};

#endif",

        ["nagule/variant.glsl"] =
@"#ifndef NAGULE_OBJECT
#define NAGULE_OBJECT
layout(std140) uniform Object {
    mat4 ObjectToWorld;
    bool IsVariant;
};
#endif",

        ["nagule/instancing.glsl"] =
@"#ifndef NAGULE_INSTANCING
#define NAGULE_INSTANCING

#define ENABLE_INSTANCING \
    ObjectToWorld = IsVariant ? VariantObjectToWorld : InstanceObjectToWorld;

layout(std140) uniform Object {
    mat4 VariantObjectToWorld;
    bool IsVariant;
};

layout(location = 4) in mat4 InstanceObjectToWorld;
mat4 ObjectToWorld;

#endif"
    };

    public string Desugar(string source)
        => Regex.Replace(source, "(\\#include \\<(?<file>.+)\\>)", match => {
            var filePath = match.Groups["file"].Value;
            if (!_internalShaderFiles.TryGetValue(filePath, out var result)) {
                Console.WriteLine("Shader file not found: " + match.Value);
                return "";
            }
            return result;
        });

    protected override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref ShaderProgram shaderProgram, ref ShaderProgramData data, bool updating)
    {
        if (updating) {
            GL.DeleteProgram(data.Handle);
        }

        var resource = shaderProgram.Resource;
        var shaders = resource.Shaders;

        // create program

        int program = GL.CreateProgram();
        var shaderHandles = new int[shaders.Length];

        try {
            for (int i = 0; i != shaders.Length; ++i) {
                var source = shaders[i];
                if (source == null) { continue; }

                int handle = CompileShader((ShaderType)i, source);
                if (handle != 0) {
                    GL.AttachShader(program, handle);
                }
                shaderHandles[i] = handle;
            }
        }
        catch {
            GL.DeleteProgram(program);
            throw;
        }

        // apply other settings

        if (resource.TransformFeedbackVaryings != null) {
            GL.TransformFeedbackVaryings(program,
                resource.TransformFeedbackVaryings.Length, resource.TransformFeedbackVaryings,
                TransformFeedbackMode.SeparateAttribs);
        }

        // link program

        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var success);

        if (success == 0) {
            string infoLog = GL.GetProgramInfoLog(program);
            Console.WriteLine(infoLog);
        }

        // detach shaders

        for (int i = 0; i != shaderHandles.Length; ++i) {
            int handle = shaderHandles[i];
            if (handle != 0) {
                GL.DetachShader(program, handle);
                GL.DeleteShader(handle);
            }
        }

        // create handles component

        var textureLocations = new EnumArray<TextureType, int>();
        for (int i = 0; i != textureLocations.Length; i++) {
            textureLocations[i] = GL.GetUniformLocation(program, Enum.GetName((TextureType)i)! + "Tex");
        }

        var uniforms = new UniformLocations {
            Textures = textureLocations,
            CameraBlock = GL.GetUniformBlockIndex(program, "Camera"),
            MainLightBlock = GL.GetUniformBlockIndex(program, "MainLight"),
            MaterialBlock = GL.GetUniformBlockIndex(program, "Material"),
            MeshBlock = GL.GetUniformBlockIndex(program, "Mesh"),
            ObjectBlock = GL.GetUniformBlockIndex(program, "Object")
        };

        GL.UniformBlockBinding(program, uniforms.CameraBlock, (int)UniformBlockBinding.Camera);
        GL.UniformBlockBinding(program, uniforms.MainLightBlock, (int)UniformBlockBinding.MainLight);
        GL.UniformBlockBinding(program, uniforms.MaterialBlock, (int)UniformBlockBinding.Material);
        GL.UniformBlockBinding(program, uniforms.MeshBlock, (int)UniformBlockBinding.Mesh);
        GL.UniformBlockBinding(program, uniforms.ObjectBlock, (int)UniformBlockBinding.Object);

        data.Handle = program;
        data.UniformLocations = uniforms;
    }

    private int CompileShader(ShaderType type, string source)
    {
        OpenTK.Graphics.OpenGL4.ShaderType glShaderType = type switch {
            ShaderType.Fragment => OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader,
            ShaderType.Vertex => OpenTK.Graphics.OpenGL4.ShaderType.VertexShader,
            ShaderType.Geometry => OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader,
            ShaderType.ComputeShader => OpenTK.Graphics.OpenGL4.ShaderType.ComputeShader,
            ShaderType.TessellationEvaluation => OpenTK.Graphics.OpenGL4.ShaderType.TessEvaluationShader,
            ShaderType.TessellationControl => OpenTK.Graphics.OpenGL4.ShaderType.TessControlShader,
            _ => throw new NotSupportedException("Unknown shader type: " + type)
        };

        int handle = GL.CreateShader(glShaderType);
        GL.ShaderSource(handle, Desugar(source));

        GL.CompileShader(handle);
        GL.GetShader(handle, ShaderParameter.CompileStatus, out int success);

        if (success == 0) {
            string infoLog = GL.GetShaderInfoLog(handle);
            Console.WriteLine(infoLog);
            GL.DeleteShader(handle);
            return 0;
        }
        return handle;
    }

    protected override void Uninitialize(
        IDataLayer<IComponent> context, Guid id, in ShaderProgram shaderProgram, in ShaderProgramData data)
    {
        GL.DeleteProgram(data.Handle);
    }
}
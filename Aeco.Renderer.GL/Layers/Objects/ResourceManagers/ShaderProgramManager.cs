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
layout(std140) uniform Object {
    mat4 ObjectToWorld;
    mat4 WorldToObject;
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
layout(std140) uniform MainLight {
    vec3 MainLightDirection;
    vec4 MainLightColor;
};
#endif",
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

        // specify sources

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

        GL.ShaderSource(vertexShader, Desugar(resource.VertexShader));
        GL.ShaderSource(fragmentShader, Desugar(resource.FragmentShader));

        // compile vertex shader

        GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);

        if (success == 0) {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }

        // compile fragment shader

        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);

        if (success == 0) {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }

        // create program handle

        int program = GL.CreateProgram();

        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
        if (success == 0) {
            string infoLog = GL.GetProgramInfoLog(program);
            Console.WriteLine(infoLog);
        }

        // detach shaders

        GL.DetachShader(program, vertexShader);
        GL.DetachShader(program, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // create handles component
        var textureLocations = new EnumArray<TextureType, int>();
        for (int i = 0; i != textureLocations.Count; i++) {
            textureLocations[i] = GL.GetUniformLocation(program, Enum.GetName((TextureType)i)! + "Tex");
        }

        var uniforms = new UniformLocations {
            Textures = textureLocations,
            CameraBlock = GL.GetUniformBlockIndex(program, "Camera"),
            ObjectBlock = GL.GetUniformBlockIndex(program, "Object"),
            MaterialBlock = GL.GetUniformBlockIndex(program, "Material"),
            MainLightBlock = GL.GetUniformBlockIndex(program, "MainLight"),
        };

        GL.UniformBlockBinding(program, uniforms.CameraBlock, 1);
        GL.UniformBlockBinding(program, uniforms.ObjectBlock, 2);
        GL.UniformBlockBinding(program, uniforms.MaterialBlock, 3);
        GL.UniformBlockBinding(program, uniforms.MainLightBlock, 4);

        data.Handle = program;
        data.UniformLocations = uniforms;
    }

    protected override void Uninitialize(
        IDataLayer<IComponent> context, Guid id, in ShaderProgram shaderProgram, in ShaderProgramData data)
    {
        GL.DeleteProgram(data.Handle);
    }
}
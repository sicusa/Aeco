namespace Aeco.Renderer.GL;

using System.Collections.Immutable;
using System.Text.RegularExpressions;

using OpenTK.Graphics.OpenGL4;

using System;
using System.Numerics;

public class ShaderProgramManager : ResourceManagerBase<ShaderProgram, ShaderProgramData, ShaderProgramResource>
{
    private static readonly Dictionary<string, string> s_internalShaderFiles = new() {
        ["nagule/common.glsl"] = 
@"#ifndef NAGULE_COMMON
#define NAGULE_COMMON

layout(std140) uniform Framebuffer {
    int ViewportWidth;
    int ViewportHeight;
};

layout(std140) uniform Camera {
    mat4 Matrix_V;
    mat4 Matrix_P;
    mat4 Matrix_VP;
    mat4 Matrix_Prev_VP;
    vec3 CameraPosition;
    float CameraNearPlaneDistance;
    float CameraFarPlaneDistance;
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
    float ShininessStrength;
    float Reflectivity;
    float Opacity;
    vec2 Tiling;
    vec2 Offset;
};

layout(std140) uniform Mesh {
    vec3 BoundingBoxMin;
    vec3 BoundingBoxMax;
};

float LinearizeDepth(float depth)
{
    float n = CameraNearPlaneDistance;
    float f = CameraFarPlaneDistance;
    return (2.0 * n) / (f + n - depth * (f - n));
}

vec3 GetViewSpacePositionFromDepth(float depth, vec2 uv)
{
    float x = uv.x * 2 - 1;
    float y = (1 - uv.y) * 2 - 1;
    vec4 projPos = vec4(x, y, depth, 1);
    vec4 pos = projPos * inverse(Matrix_P);
    return pos.xyz / pos.w;
}

vec3 GetWorldSpacePositionFromDepth(float depth, vec2 uv)
{
    float x = uv.x * 2 - 1;
    float y = (1 - uv.y) * 2 - 1;
    vec4 projPos = vec4(x, y, depth, 1);
    vec4 pos = projPos * inverse(Matrix_VP);
    return pos.xyz / pos.w;
}

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

#endif",

        ["nagule/transparency.glsl"] =
@"#ifndef NAGULE_TRANSPARENCY
#define NAGULE_TRANSPARENCY

float GetTransparencyDepthWeight(float z, float a)
{
    return clamp(pow(min(1.0, a * 10.0) + 0.01, 3.0) * 1e8 * pow(1.0 - z * 0.9, 3.0), 1e-2, 3e3);
}

float GetBlendAlpha(float a)
{
    return GetTransparencyDepthWeight(gl_FragCoord.z, a) * a;
}

#endif",

        ["nagule/blinn_phong.glsl"] =
@"#ifndef NAGULE_BLINN_PHONG
#define NAGULE_BLINN_PHONG

#include <nagule/common.glsl>

vec4 BlinnPhong(vec3 position, vec2 texCoord, vec3 normal)
{
    vec2 tiledCoord = texCoord * Tiling;

    vec3 lightDir = MainLightDirection;
    vec3 lightColor = MainLightColor.rgb * MainLightColor.a;
    vec3 ambientColor = Ambient.rgb * Ambient.a;

    // diffuse 
    float diff = max(0.5 * dot(normal, -lightDir) + 0.5, 0.0);
    vec4 diffuseColor = Diffuse * texture(DiffuseTex, tiledCoord);
    vec3 diffuse = (diff * lightColor + ambientColor) * diffuseColor.rgb;
    
    // specular
    vec3 viewDir = normalize(CameraPosition - position);
    vec3 divisor = normalize(viewDir - lightDir);
    float spec = pow(max(dot(divisor, normal), 0.0), Shininess);
    vec4 specularColor = Specular * texture(SpecularTex, tiledCoord);
    vec3 specular = spec * lightColor * specularColor.rgb;

    // emission
    vec4 emissionColor = Emission * texture(EmissionTex, tiledCoord);
    vec3 emission = emissionColor.rgb * emissionColor.a;

    return vec4(diffuse + specular + emission, diffuseColor.a);
}

#endif"
    };


    private static readonly Dictionary<Type, Action<int, object>> s_uniformSetters = new() {
        [typeof(int)] = (location, value) => GL.Uniform1(location, (int)value),
        [typeof(int[])] = (location, value) => {
            var arr = (int[])value;
            GL.Uniform1(location, arr.Length, arr);
        },
        [typeof(float)] = (location, value) => GL.Uniform1(location, (float)value),
        [typeof(float[])] = (location, value) => {
            var arr = (float[])value;
            GL.Uniform1(location, arr.Length, arr);
        },
        [typeof(double)] = (location, value) => GL.Uniform1(location, (double)value),
        [typeof(double[])] = (location, value) => {
            var arr = (double[])value;
            GL.Uniform1(location, arr.Length, arr);
        },
        [typeof(Vector2)] = (location, value) => {
            var vec = (Vector2)value;
            GL.Uniform2(location, vec.X, vec.Y);
        },
        [typeof(Vector2[])] = (location, value) => {
            var arr = (Vector2[])value;
            GL.Uniform2(location, arr.Length, ref arr[0].X);
        },
        [typeof(Vector3)] = (location, value) => {
            var vec = (Vector3)value;
            GL.Uniform3(location, vec.X, vec.Y, vec.Z);
        },
        [typeof(Vector3[])] = (location, value) => {
            var arr = (Vector3[])value;
            GL.Uniform3(location, arr.Length, ref arr[0].X);
        },
        [typeof(Vector4)] = (location, value) => {
            var vec = (Vector4)value;
            GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        },
        [typeof(Vector4[])] = (location, value) => {
            var arr = (Vector4[])value;
            GL.Uniform3(location, arr.Length, ref arr[0].X);
        },
        [typeof(Matrix3x2)] = (location, value) => {
            var mat = (Matrix3x2)value;
            GL.UniformMatrix2x3(location, 1, true, ref mat.M11);
        },
        [typeof(Matrix3x2[])] = (location, value) => {
            var arr = (Matrix3x2[])value;
            GL.UniformMatrix2x3(location, arr.Length, true, ref arr[0].M11);
        },
        [typeof(Matrix4x4)] = (location, value) => {
            var mat = (Matrix4x4)value;
            GL.UniformMatrix4(location, 1, true, ref mat.M11);
        },
        [typeof(Matrix4x4[])] = (location, value) => {
            var arr = (Matrix4x4[])value;
            GL.UniformMatrix4(location, arr.Length, true, ref arr[0].M11);
        },
    };

    public string Desugar(string source)
        => Regex.Replace(source, "(\\#include \\<(?<file>.+)\\>)", match => {
            var filePath = match.Groups["file"].Value;
            if (!s_internalShaderFiles.TryGetValue(filePath, out var result)) {
                Console.WriteLine("Shader file not found: " + match.Value);
                return "";
            }
            return Desugar(result);
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
                TransformFeedbackMode.InterleavedAttribs);
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

        // initialize uniform locations

        data.DepthBufferLocation = GL.GetUniformLocation(program, "DepthBuffer");

        EnumArray<TextureType, int>? textureLocations = null;
        if (resource.IsMaterialTexturesEnabled) {
            textureLocations = new EnumArray<TextureType, int>();
            for (int i = 0; i != textureLocations.Length; i++) {
                textureLocations[i] = GL.GetUniformLocation(program, Enum.GetName((TextureType)i)! + "Tex");
            }
        }

        var customLocations = ImmutableDictionary<string, int>.Empty;
        if (resource.CustomUniforms != null) {
            var builder = ImmutableDictionary.CreateBuilder<string, int>();
            foreach (var uniform in resource.CustomUniforms) {
                var location = GL.GetUniformLocation(program, uniform);
                if (location == -1) {
                    Console.WriteLine($"Custom uniform '{uniform}' not found");
                    continue;
                }
                builder.Add(uniform, location);
            }
            customLocations = builder.ToImmutable();
        }

        if (resource.DefaultUniformValues != null) {
            foreach (var (name, value) in resource.DefaultUniformValues) {
                var location = GL.GetUniformLocation(program, name);
                if (location == -1) {
                    Console.WriteLine($"Failed to set uniform '{name}': uniform not found.");
                }
                if (!s_uniformSetters.TryGetValue(value.GetType(), out var setter)) {
                    Console.WriteLine($"Failed to set uniform '{name}': uniform type unrecognized.");
                    continue;
                }
                try {
                    setter(location, value);
                }
                catch (Exception e) {
                    Console.WriteLine($"Failed to set uniform '{name}': " + e.Message);
                }
            }
        }

        EnumArray<ShaderType, ImmutableDictionary<string, int>>? subroutineIndeces = null;
        if (resource.Subroutines != null) {
            foreach (var (shaderType, name) in resource.Subroutines) {
                var index = GL.GetSubroutineIndex(program, ToGLShaderType(shaderType), name);
                if (index == -1) {
                    Console.WriteLine($"Subroutine index '{name}' not found");
                    continue;
                }
                subroutineIndeces = subroutineIndeces ?? new();
                var indeces = subroutineIndeces[shaderType] ?? ImmutableDictionary<string, int>.Empty;
                indeces = indeces.Add(name, index);
                subroutineIndeces[shaderType] = indeces;
            }
        }

        var blockLocations = new BlockLocations {
            FramebufferBlock = GL.GetUniformBlockIndex(program, "Framebuffer"),
            CameraBlock = GL.GetUniformBlockIndex(program, "Camera"),
            MainLightBlock = GL.GetUniformBlockIndex(program, "MainLight"),
            MaterialBlock = GL.GetUniformBlockIndex(program, "Material"),
            MeshBlock = GL.GetUniformBlockIndex(program, "Mesh"),
            ObjectBlock = GL.GetUniformBlockIndex(program, "Object")
        };

        if (blockLocations.FramebufferBlock != -1) {
            GL.UniformBlockBinding(program, blockLocations.FramebufferBlock, (int)UniformBlockBinding.Framebuffer);
        }
        if (blockLocations.CameraBlock != -1) {
            GL.UniformBlockBinding(program, blockLocations.CameraBlock, (int)UniformBlockBinding.Camera);
        }
        if (blockLocations.MainLightBlock != -1) {
            GL.UniformBlockBinding(program, blockLocations.MainLightBlock, (int)UniformBlockBinding.MainLight);
        }
        if (blockLocations.MaterialBlock != -1) {
            GL.UniformBlockBinding(program, blockLocations.MaterialBlock, (int)UniformBlockBinding.Material);
        }
        if (blockLocations.MeshBlock != -1) {
            GL.UniformBlockBinding(program, blockLocations.MeshBlock, (int)UniformBlockBinding.Mesh);
        }
        if (blockLocations.ObjectBlock != -1) {
            GL.UniformBlockBinding(program, blockLocations.ObjectBlock, (int)UniformBlockBinding.Object);
        }

        // finish initialization

        data.Handle = program;
        data.TextureLocations = textureLocations;
        data.CustomLocations = customLocations;
        data.SubroutineLoactions = subroutineIndeces;
        data.BlockLocations = blockLocations;
    } 

    private int CompileShader(ShaderType type, string source)
    {
        var glShaderType = ToGLShaderType(type);
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

    private OpenTK.Graphics.OpenGL4.ShaderType ToGLShaderType(ShaderType type)
        => type switch {
            ShaderType.Fragment => OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader,
            ShaderType.Vertex => OpenTK.Graphics.OpenGL4.ShaderType.VertexShader,
            ShaderType.Geometry => OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader,
            ShaderType.Compute => OpenTK.Graphics.OpenGL4.ShaderType.ComputeShader,
            ShaderType.TessellationEvaluation => OpenTK.Graphics.OpenGL4.ShaderType.TessEvaluationShader,
            ShaderType.TessellationControl => OpenTK.Graphics.OpenGL4.ShaderType.TessControlShader,
            _ => throw new NotSupportedException("Unknown shader type: " + type)
        };
}
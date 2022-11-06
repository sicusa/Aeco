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

vec3 GetClipSpacePositionFromDepth(float depth, vec2 uv)
{
    float z = depth * 2.0 - 1.0;
    return vec3(uv * 2.0 - 1.0, z);
}

vec4 GetViewSpacePositionFromDepth(float depth, vec2 uv)
{
    float z = depth * 2.0 - 1.0;
    vec4 clipPos = vec4(uv * 2.0 - 1.0, z, 1.0);
    vec4 viewPos = clipPos * inverse(Matrix_P);
    viewPos.xyz /= viewPos.w;
    return viewPos;
}

vec4 GetWorldSpacePositionFromDepth(float depth, vec2 uv)
{
    float z = depth * 2.0 - 1.0;
    vec4 clipPos = vec4(uv * 2.0 - 1.0, z, 1.0);
    vec4 viewPos = clipPos * inverse(Matrix_P);
    viewPos.xyz /= viewPos.w;
    return viewPos * inverse(Matrix_V);
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

float GetTransparencyWeight(float z, float a) {
    return a * max(0.01, min(3e3, 10 / (1e-5 + z * z * 0.25 + pow(z / 200, 6))));
}

float GetTransparencyAlpha(float a) {
    return GetTransparencyWeight(gl_FragCoord.z, a) * a;
}

#endif",

        ["nagule/lighting.glsl"] =
@"#ifndef NAGULE_LIGHTING
#define NAGULE_LIGHTING

#include <nagule/common.glsl>

#define TILE_VERTICAL_COUNT 8
#define TILE_HORIZONTAL_COUNT 8
#define TILE_TOTAL_COUNT TILE_VERTICAL_COUNT * TILE_HORIZONTAL_COUNT
#define TILE_MAXIMUM_LIGHT_COUNT 8

#define LIGHT_NONE          0
#define LIGHT_AMBIENT       1
#define LIGHT_DIRECTIONAL   2
#define LIGHT_POINT         3
#define LIGHT_SPOT          4
#define LIGHT_AREA          5

#define LIGHT_COMPONENT_COUNT 19

struct Light {
    int Category;       // 4    1
    vec4 Color;         // 16   5
    vec3 Position;      // 12   8
    vec3 Direction;     // 12   11
    vec3 Up;            // 12   14

    float AttenuationConstant;  // 4    15
    float AttenuationLinear;    // 4    16
    float AttenuationQuadratic; // 4    17

    vec2 ConeAnglesOrAreaSize; // 8 19
};

struct LightingResult {
    float Diffuse;
    float Specular;
};

layout(std140) uniform Lighting {
    int LightIndeces[TILE_TOTAL_COUNT * TILE_MAXIMUM_LIGHT_COUNT];
};

uniform samplerBuffer LightBuffer;

int GetTileIndex(vec2 fragCoord)
{
    int x = int(floor((fragCoord.x - 0.5) / ViewportWidth * TILE_HORIZONTAL_COUNT));
    int y = int(floor((fragCoord.y - 0.5) / ViewportHeight * TILE_VERTICAL_COUNT));
    return y * TILE_HORIZONTAL_COUNT + x;
}

Light GetLight(int index)
{
    int offset = index * LIGHT_COMPONENT_COUNT;
    Light light;

    int category = int(texelFetch(LightBuffer, offset).r);
    light.Category = category;

    light.Color = vec4(
        texelFetch(LightBuffer, offset + 1).r,
        texelFetch(LightBuffer, offset + 2).r,
        texelFetch(LightBuffer, offset + 3).r,
        texelFetch(LightBuffer, offset + 4).r);

    light.Position = vec3(
        texelFetch(LightBuffer, offset + 5).r,
        texelFetch(LightBuffer, offset + 6).r,
        texelFetch(LightBuffer, offset + 7).r);
    light.Direction = vec3(
        texelFetch(LightBuffer, offset + 8).r,
        texelFetch(LightBuffer, offset + 9).r,
        texelFetch(LightBuffer, offset + 10).r);
    light.Up = vec3(
        texelFetch(LightBuffer, offset + 11).r,
        texelFetch(LightBuffer, offset + 12).r,
        texelFetch(LightBuffer, offset + 13).r);

    light.AttenuationConstant = texelFetch(LightBuffer, offset + 14).r;
    light.AttenuationLinear = texelFetch(LightBuffer, offset + 15).r;
    light.AttenuationQuadratic = texelFetch(LightBuffer, offset + 16).r;

    light.ConeAnglesOrAreaSize = vec2(
        texelFetch(LightBuffer, offset + 17).r,
        texelFetch(LightBuffer, offset + 18).r);

    return light;
}

float CalculateLightAttenuation(Light light, float distance)
{
    return 1 / (
        light.AttenuationConstant +
        light.AttenuationLinear * distance +
        light.AttenuationQuadratic * distance * distance);
}

LightingResult CalculateLightingResult(vec3 position, vec3 normal, Light light)
{
    LightingResult r;
    int category = light.Category;

    if (category == LIGHT_AMBIENT) {
        r.Diffuse = light.Diffuse;
        r.Specular = 0;
    }
    else if (category == LIGHT_DIRECTIONAL) {
        vec3 lightDir = light.Direction;

        // diffuse
        r.Diffuse = max(0.5 * dot(normal, -lightDir) + 0.5, 0.0);

        // specular
        vec3 viewDir = normalize(CameraPosition - position);
        vec3 divisor = normalize(viewDir - lightDir);
        float spec = pow(max(dot(divisor, normal), 0.0), Shininess);
        r.Specular = spec;
    }
    else {
        vec3 lightDir = position - light.Position;
        float distance = length(lightDir);
        float attenuation = CalculateLightAttenuation(light, distance);
        lightDir /= distance;

        if (category == LIGHT_POINT) {
            // diffuse
            float diff = max(0.5 * dot(normal, -lightDir) + 0.5, 0.0);
            r.Diffuse = diff;

            // specular
            vec3 viewDir = normalize(CameraPosition - position);
            vec3 divisor = normalize(viewDir - lightDir);
            float spec = pow(max(dot(divisor, normal), 0.0), Shininess);
            r.Specular = spec;
        }
    }
}

#endif",

        ["nagule/blinn_phong.glsl"] =
@"#ifndef NAGULE_BLINN_PHONG
#define NAGULE_BLINN_PHONG

#include <nagule/common.glsl>
#include <nagule/lighting.glsl>

uniform sampler2D DiffuseTex;
uniform sampler2D SpecularTex;
uniform sampler2D EmissionTex;

vec4 BlinnPhong(vec3 position, vec2 texCoord, vec3 normal)
{
    vec2 tiledCoord = texCoord * Tiling;
    vec4 diffuseColor = Diffuse * texture(DiffuseTex, tiledCoord);
    vec4 specularColor = Specular * texture(SpecularTex, tiledCoord);

    vec3 diffuse = vec3(0);
    vec3 specular = vec3(0);

    Light light = GetLight(0);
    vec3 lightColor = light.Color.rgb * light.Color.a;

    int category = light.Category;

    // emission
    vec4 emissionColor = Emission * texture(EmissionTex, tiledCoord);
    vec3 emission = emissionColor.rgb * emissionColor.a;

    return vec4(diffuse * diffuseColor.rgb + specular * specularColor.rgb + emission, diffuseColor.a);
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

        data.LightBufferLocation = GL.GetUniformLocation(program, "LightBuffer");
        data.DepthBufferLocation = GL.GetUniformLocation(program, "DepthBuffer");

        EnumArray<TextureType, int>? textureLocations = null;
        if (resource.IsMaterialTexturesEnabled) {
            textureLocations = new EnumArray<TextureType, int>();
            for (int i = 0; i != textureLocations.Length - 1; i++) {
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
            subroutineIndeces = new();

            ShaderType shaderType = 0;
            foreach (var names in resource.Subroutines) {
                var indeces = subroutineIndeces[shaderType] ?? ImmutableDictionary<string, int>.Empty;
                if (names == null) {
                    subroutineIndeces[shaderType] = indeces;
                    continue;
                }
                foreach (var name in names) {
                    var index = GL.GetSubroutineIndex(program, ToGLShaderType(shaderType), name);
                    if (index == -1) {
                        Console.WriteLine($"Subroutine index '{name}' not found");
                        continue;
                    }
                    indeces = indeces.Add(name, index);
                }
                subroutineIndeces[shaderType] = indeces;
                ++shaderType;
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
            GL.UniformBlockBinding(program, blockLocations.FramebufferBlock, (int)UniformBlockBinding.RenderTarget);
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
        data.SubroutineIndeces = subroutineIndeces;
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
#ifndef AECO_LIGHTING
#define AECO_LIGHTING

#include <aeco/common.glsl>

#define CLUSTER_COUNT_X 16
#define CLUSTER_COUNT_Y 9
#define CLUSTER_COUNT_Z 24
#define CLUSTER_COUNT (CLUSTER_COUNT_X * CLUSTER_COUNT_Y * CLUSTER_COUNT_Z)
#define MAXIMUM_CLUSTER_LIGHT_COUNT 64

#define LIGHT_NONE          0
#define LIGHT_AMBIENT       1
#define LIGHT_DIRECTIONAL   2
#define LIGHT_POINT         3
#define LIGHT_SPOT          4
#define LIGHT_AREA          5

#define LIGHT_COMPONENT_COUNT 19

layout(std140) uniform LightingEnv {
    float ClusterDepthSliceMultiplier;
    float ClusterDepthSliceSubstractor;
};

struct Light {
    int Category;
    vec4 Color;
    vec3 Position;
    vec3 Direction;
    vec3 Up;

    float AttenuationConstant;
    float AttenuationLinear;
    float AttenuationQuadratic;

    vec2 ConeCutoffsOrAreaSize;
};

uniform samplerBuffer LightsBuffer;
uniform isamplerBuffer ClustersBuffer;
uniform isamplerBuffer ClusterLightCountsBuffer;

int CalculateClusterDepthSlice(float z) {
    return max(int(floor(log2(z) * ClusterDepthSliceMultiplier - ClusterDepthSliceSubstractor)), 0);
}

int GetClusterIndex(vec3 fragCoord)
{
    int depthSlice = CalculateClusterDepthSlice(LinearizeDepth(fragCoord.z)) - 1;
    float tileSizeX = ViewportWidth / CLUSTER_COUNT_X;
    float tileSizeY = ViewportHeight / CLUSTER_COUNT_Y;

    return int(fragCoord.x / tileSizeX)
        + CLUSTER_COUNT_X * int(fragCoord.y / tileSizeY)
        + (CLUSTER_COUNT_X * CLUSTER_COUNT_Y) * depthSlice;
}

int FetchLightIndex(int cluster, int offset) {
    return texelFetch(ClustersBuffer, cluster * MAXIMUM_CLUSTER_LIGHT_COUNT + offset).r;
}

int FetchLightCount(int index) {
    return texelFetch(ClusterLightCountsBuffer, index).r;
}

Light FetchLight(int index)
{
    int offset = index * LIGHT_COMPONENT_COUNT;
    Light light;

    int category = int(texelFetch(LightsBuffer, offset).r);
    light.Category = category;

    light.Color = vec4(
        texelFetch(LightsBuffer, offset + 1).r,
        texelFetch(LightsBuffer, offset + 2).r,
        texelFetch(LightsBuffer, offset + 3).r,
        texelFetch(LightsBuffer, offset + 4).r);

    light.Position = vec3(
        texelFetch(LightsBuffer, offset + 5).r,
        texelFetch(LightsBuffer, offset + 6).r,
        texelFetch(LightsBuffer, offset + 7).r);
    light.Direction = vec3(
        texelFetch(LightsBuffer, offset + 8).r,
        texelFetch(LightsBuffer, offset + 9).r,
        texelFetch(LightsBuffer, offset + 10).r);
    light.Up = vec3(
        texelFetch(LightsBuffer, offset + 11).r,
        texelFetch(LightsBuffer, offset + 12).r,
        texelFetch(LightsBuffer, offset + 13).r);

    light.AttenuationConstant = texelFetch(LightsBuffer, offset + 14).r;
    light.AttenuationLinear = texelFetch(LightsBuffer, offset + 15).r;
    light.AttenuationQuadratic = texelFetch(LightsBuffer, offset + 16).r;

    light.ConeCutoffsOrAreaSize = vec2(
        texelFetch(LightsBuffer, offset + 17).r,
        texelFetch(LightsBuffer, offset + 18).r);

    return light;
}

Light FetchLightFromCluster(int cluster, int offset) {
    return FetchLight(FetchLightIndex(cluster, offset));
}

float CalculateLightAttenuation(Light light, float distance)
{
    return 1 / (
        light.AttenuationConstant +
        light.AttenuationLinear * distance +
        light.AttenuationQuadratic * distance * distance);
}

#endif
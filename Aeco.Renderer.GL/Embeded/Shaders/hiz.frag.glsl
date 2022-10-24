#version 410 core

uniform sampler2D LastMaxMip;
uniform sampler2D LastMinMip;
uniform ivec2 LastMipSize;

in vec2 TexCoord;
layout(location = 0) out float MinDepth;

void main()
{
	vec4 texels;

	texels.x = texture(LastMaxMip, TexCoord).x;
	texels.y = textureOffset(LastMaxMip, TexCoord, ivec2(-1, 0)).x;
	texels.z = textureOffset(LastMaxMip, TexCoord, ivec2(-1,-1)).x;
	texels.w = textureOffset(LastMaxMip, TexCoord, ivec2(0,-1)).x;
	float maxZ = max(max(texels.x, texels.y), max(texels.z, texels.w));

	texels.x = texture(LastMinMip, TexCoord).x;
	texels.y = textureOffset(LastMinMip, TexCoord, ivec2(-1, 0)).x;
	texels.z = textureOffset(LastMinMip, TexCoord, ivec2(-1,-1)).x;
	texels.w = textureOffset(LastMinMip, TexCoord, ivec2(0,-1)).x;
	float minZ = min(min(texels.x, texels.y), min(texels.z, texels.w));

	vec3 extra;
	// if we are reducing an odd-width texture then the edge fragments have to fetch additional texels
	if (((LastMipSize.x & 1) != 0) && (int(gl_FragCoord.x) == LastMipSize.x-3)) {
		// if both edges are odd, fetch the top-left corner texel
		if (((LastMipSize.y & 1) != 0) && (int(gl_FragCoord.y) == LastMipSize.y-3)) {
			extra.z = textureOffset(LastMaxMip, TexCoord, ivec2(1, 1)).x;
			maxZ = max(maxZ, extra.z);
			extra.z = textureOffset(LastMinMip, TexCoord, ivec2(1, 1)).x;
			minZ = min(minZ, extra.z);
		}
		extra.x = textureOffset(LastMaxMip, TexCoord, ivec2(1, 0)).x;
		extra.y = textureOffset(LastMaxMip, TexCoord, ivec2(1,-1)).x;
		maxZ = max(maxZ, max(extra.x, extra.y));

		extra.x = textureOffset(LastMinMip, TexCoord, ivec2(1, 0)).x;
		extra.y = textureOffset(LastMinMip, TexCoord, ivec2(1,-1)).x;
		minZ = min(minZ, min(extra.x, extra.y));
	} else
	// if we are reducing an odd-height texture then the edge fragments have to fetch additional texels
	if (((LastMipSize.y & 1) != 0) && (int(gl_FragCoord.y) == LastMipSize.y-3)) {
		extra.x = textureOffset(LastMaxMip, TexCoord, ivec2(0, 1)).x;
		extra.y = textureOffset(LastMaxMip, TexCoord, ivec2(-1, 1)).x;
		maxZ = max(maxZ, max(extra.x, extra.y));

		extra.x = textureOffset(LastMinMip, TexCoord, ivec2(0, 1)).x;
		extra.y = textureOffset(LastMinMip, TexCoord, ivec2(-1, 1)).x;
		minZ = min(minZ, max(extra.x, extra.y));
	}

	gl_FragDepth = maxZ;
	MinDepth = minZ;
}

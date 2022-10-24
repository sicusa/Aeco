#version 410 core

layout(points) in;
layout(triangle_strip, max_vertices = 1024) out;

uniform ivec2 TileCount;

out vec2 TexCoord;
flat out int TileIndex;

void main()
{
    float dx = 2.0 / TileCount.x;
    float dy = 2.0 / TileCount.y;

    for (int r = 0; r < TileCount.x; r++) {
        float left = -1 + r * dx;
        float right = left + dx;

        for (int c = 0; c < TileCount.y; c++) {
            float bottom = -1 + c * dy;
            float top = bottom + dy;
            int index = r * TileCount.y + c;

            gl_Position = vec4(right, top, 0.5, 1.0);
            TexCoord = vec2(1.0, 1.0);
            TileIndex = index;
            EmitVertex();

            gl_Position = vec4(left, top, 0.5, 1.0);
            TexCoord = vec2(0.0, 1.0); 
            TileIndex = index;
            EmitVertex();

            gl_Position = vec4(right, bottom, 0.5, 1.0);
            TexCoord = vec2(1.0, 0.0); 
            TileIndex = index;
            EmitVertex();

            gl_Position = vec4(left, bottom, 0.5, 1.0);
            TexCoord = vec2(0.0, 0.0); 
            TileIndex = index;
            EmitVertex();

            EndPrimitive(); 
        }
    }
}


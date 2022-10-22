#version 410 core

uniform sampler2D ColorBuffer;
uniform sampler2D DepthBuffer;

in vec2 TexCoord;
out vec4 FragColor;

void main()
{
    FragColor = texture(ColorBuffer, TexCoord);
}
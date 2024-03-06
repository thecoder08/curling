#version 330

out vec4 outputColor;

uniform highp vec3 color;
uniform highp vec3 lightDir;
out vec3 dummy;

void main()
{
    dummy = lightDir;
    outputColor = vec4(color, 1.0);
}
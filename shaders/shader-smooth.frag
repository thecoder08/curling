#version 330

in vec3 fragNormal;

out vec4 outputColor;

uniform highp vec3 lightDir;
uniform highp vec3 color;

void main()
{
    float intensity = max(dot(normalize(fragNormal), lightDir), 0.3);
    outputColor = vec4(color * intensity, 1.0);
}
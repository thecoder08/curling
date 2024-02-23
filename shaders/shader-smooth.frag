#version 330

in vec3 fragNormal;

out vec4 outputColor;

uniform vec3 lightDir;
uniform vec3 color;

void main()
{
    float intensity = dot(normalize(fragNormal), normalize(lightDir));
    intensity = clamp(intensity, 0.0, 1.0);
    outputColor = vec4(color * intensity, 1.0);
}
#version 330

in vec3 fragNormal;

out vec4 outputColor;

uniform vec3 lightDir;
uniform vec3 color;

void main()
{
    float intensity = max(dot(normalize(fragNormal), normalize(lightDir)), 0.0);
    //intensity = lightDir.x;
    intensity = 1.0;
    outputColor = vec4(color * intensity, 1.0);
    outputColor = vec4(fragNormal, 1.0);

}
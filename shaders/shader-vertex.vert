#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;

out vec3 vertColor;

uniform mat4 mvp;
uniform vec3 lightDir;
uniform vec3 color;

void main(void)
{
    float intensity = max(dot(aNormal, lightDir), 0.3);
    vertColor = color * intensity;
    gl_Position = vec4(aPosition, 1.0) * mvp;
}
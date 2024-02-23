#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;

out vec3 fragNormal;

uniform mat4 mvp;

void main(void)
{
    fragNormal = aNormal;
    gl_Position = vec4(aPosition, 1.0) * mvp;
}
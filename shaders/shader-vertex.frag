#version 330

out vec4 outputColor;

in vec3 vertColor;

void main()
{
    outputColor = vec4(vertColor, 1.0);
}
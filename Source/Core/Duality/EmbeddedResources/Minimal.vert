#include "/shaders/core"
layout(location = ATTRIB_POSITION) in vec3 iPosition;

uniform mat4x4 modelViewProjection;

void main()
{
	gl_Position = modelViewProjection * vec4(iPosition, 1);
}
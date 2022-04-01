#include "/shaders/core"

uniform float time;
uniform vec2 uvAnimation;

uniform mat4x4 itWorld;

layout(location = ATTRIB_POSITION) in vec3 iPosition;
layout(location = ATTRIB_TEXCOORD_0) in vec2 iTexCoord;
#ifdef SPLAT
layout(location = ATTRIB_TEXCOORD_1) in vec2 iTexCoord2;
#endif
layout(location = ATTRIB_NORMAL) in vec3 iNormal;
layout(location = ATTRIB_TANGENT) in vec3 iTangent;

#ifdef SKINNED
layout(location = ATTRIB_BONE_INDEX) in vec4 iBoneIndex;
layout(location = ATTRIB_BONE_WEIGHT) in vec4 iBoneWeight;
#endif

out vec3 normal;
out vec3 tangent;
out vec3 bitangent;
out vec2 texCoord;
#ifdef SPLAT
out vec2 texCoordDetail;
#endif
out vec4 position;

uniform mat4x4 world;
uniform mat4x4 worldView;
uniform mat4x4 modelViewProjection;

#ifdef SKINNED
uniform mat4x4[96] bones;
#endif

void main()
{
	texCoord = iTexCoord;
#ifdef SPLAT
	texCoordDetail = iTexCoord2;
#endif
	
#ifdef ANIM_UV
	texCoord += uvAnimation * time;
#endif
	
#ifdef SKINNED
	vec4 blendPos = vec4(0, 0, 0, 0);
	vec3 blendNormal = vec3(0, 0, 0);
	
	for (int bone = 0; bone < 4; bone++)
	{
		int index = int(iBoneIndex[bone]);
		float weight = iBoneWeight[bone];
		
		mat4 worldMatrix = bones[index];
		blendPos += (worldMatrix * vec4(iPosition, 1)) * weight;
		
		mat3 worldRot = mat3(worldMatrix[0].xyz, worldMatrix[1].xyz, worldMatrix[2].xyz);
		blendNormal += (worldRot * iNormal) * weight;
	}
	
	blendPos = vec4(blendPos.xyz, 1);
	
	normal = normalize(blendNormal);
	tangent = normalize(iTangent);
	bitangent = cross(normal, tangent);
	
	normal = mat3(itWorld) * normal;
	tangent = mat3(itWorld) * tangent;
	bitangent = mat3(itWorld) * bitangent;
	
	position = world * blendPos;

	gl_Position = modelViewProjection * blendPos;
#else
	normal = mat3(itWorld) * normalize(iNormal);
	tangent = mat3(itWorld) * normalize(iTangent);
	bitangent = normalize(cross(normal, tangent));
	
	position = world * vec4(iPosition, 1);

	gl_Position = modelViewProjection * vec4(iPosition, 1);
#endif
}
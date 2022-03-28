#include "/shaders/core"

#include "/shaders/brdf"

uniform float time;
uniform vec2 uvAnimation;

in vec3 normal;
in vec3 tangent;
in vec3 bitangent;
in vec2 texCoord;
#ifdef SPLAT
in vec2 texCoordDetail;
#endif
in vec4 position;

layout(location = 0) out vec4 oColor;
layout(location = 1) out vec4 oNormal;
layout(location = 2) out vec4 oSpecular;

uniform mat4x4 itWorld;

uniform vec3 cameraPosition;

vec4 get_diffuse() {
vec4 v_0 = vec4(1, 0.8862745, 0.6078432, 1);
return pow(v_0, vec4(2.2));

}
vec3 get_normals() {
return normalize(normal);
}
float get_metallic() {
float f_1 = 1;
return f_1;

}
float get_roughness() {
float f_2 = 0.3;
return f_2;

}
float get_specular() {
return 0.5;
}

void main() {
	vec3 normals = get_normals();
	
	normals = normalize(mat3x3(itWorld) * normals);
	
	vec3 diffuse = get_diffuse().xyz;
	
	float metallic = get_metallic();
	float specular = get_specular();

	float roughness = get_roughness();
	
	//oColor = vec4(encodeDiffuse(diffuse), 1);
	oColor = vec4(1, 1, 1, 1);
	oNormal = vec4(encodeNormals(normals), 1);
	oSpecular = vec4(metallic, roughness, specular, 0);
	
#ifdef UNLIT
	oNormal.w = 0;
#endif
}
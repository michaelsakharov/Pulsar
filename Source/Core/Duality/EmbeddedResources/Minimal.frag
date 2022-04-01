#include "/shaders/core"

uniform float time;
uniform vec2 uvAnimation;

uniform mat4x4 itWorld;

#include "/shaders/brdf"

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
layout(location = 3) out vec4 oPosition;

uniform vec3 cameraPosition;

uniform sampler2D mainTex;
uniform sampler2D normalMap;
uniform sampler2D samplerRoughnessMetalMap;
uniform sampler2D samplerOcclusionRoughnessMetalness;

uniform vec4 mainColor;
uniform float uRoughness;
uniform float uMetalness;

void get_material(out vec3 diffuse, out vec3 normals, out float metallic, out float specular, out float roughness, out float occlusion) {
	diffuse = pow(texture(mainTex, texCoord).xyz * mainColor.xyz, vec3(2.2));
	//diffuse = texture(mainTex, texCoord).xyz * mainColor.xyz;

	occlusion = 1.0;
	
	vec4 NR = texture(normalMap, texCoord);
	NR.xyz = normalize(NR.xyz * 2.0 - 1.0);
	
	mat3x3 TBN = mat3x3(normalize(tangent), normalize(bitangent), normalize(normal));
	normals = normalize(TBN * NR.xyz);

	//normals = normalize(normal);

	//#if !defined(HAS_SAMPLER_ROUGHNESSMETALMAP) && !defined(HAS_SAMPLER_OCCLUSIONROUGHNESSMETALNESS)
	//roughness = NR.w;
	//metallic = 0.0;
	//#endif
	
#ifdef HAS_SAMPLER_ROUGHNESSMETALMAP
	vec4 materialParameters = texture(samplerRoughnessMetalMap, texCoord);
	
	roughness = materialParameters.x;
	metallic = materialParameters.y;
#endif
#ifdef HAS_SAMPLER_OCCLUSIONROUGHNESSMETALNESS
	vec4 materialParameters = texture(samplerOcclusionRoughnessMetalness, texCoord);
	
	roughness = materialParameters.y;
	metallic = materialParameters.z;
	//occlusion = materialParameters.x;
#endif
#ifdef HAS_ROUGHNESS
	roughness = uRoughness;
#endif
#ifdef HAS_METALNESS
	metallic = uMetalness;
#endif

	specular = 0.5;
}

void main() {
	vec3 diffuse;
	vec3 normals;
	float metallic, specular, roughness, occlusion;

	get_material(diffuse, normals, metallic, specular, roughness, occlusion);
	roughness = max(0.01, roughness);
	
	oColor = vec4(encodeDiffuse(diffuse), 0);
	oNormal = vec4(encodeNormals(normals), 1);
	oSpecular = vec4(max(metallic, 0.5), max(roughness, 0.5), max(specular, 0.5), occlusion);
	oPosition = position;
	
#ifdef UNLIT
	oNormal.w = 0;
#endif
}
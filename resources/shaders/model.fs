#version 330 core
layout (location = 0) out vec4 FragColor;

#define NR_FIREFLIES (3)

struct Material {
    sampler2D texture_diffuse1;
    sampler2D texture_specular1;

    float shininess;
};

struct DirLight {
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float constant;
    float linear;
    float quadratic;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float constant;
    float linear;
    float quadratic;
};

in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPos;

uniform vec3 viewPos;
uniform sampler2D texture_diffuse1;

uniform DirLight dirLight;

uniform PointLight lamp1;
uniform PointLight lamp2;
uniform PointLight[NR_FIREFLIES] fireflies;

uniform SpotLight torch;
uniform bool bTorch;

vec3 CalculateDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 tex);
vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 tex);
vec3 CalculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 tex);

uniform Material material;

void main() {

    vec4 tex = vec4(texture(texture_diffuse1, TexCoords));

    if (tex.a < 0.5)
        discard;

    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    vec3 result = vec3(0.0);

    result += CalculateDirLight(dirLight, norm, viewDir, tex.xyz);
    result += CalculatePointLight(lamp1, norm, FragPos, viewDir, tex.xyz);
    result += CalculatePointLight(lamp2, norm, FragPos, viewDir, tex.xyz);

    if (bTorch == true)
        result += CalculateSpotLight(torch, norm, FragPos, viewDir, tex.xyz);

    for (int i = 0; i < NR_FIREFLIES; i++)
      result += CalculatePointLight(fireflies[i], norm, FragPos, viewDir, tex.xyz);

    FragColor = vec4(result, 1.0);
}

vec3 CalculateDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 tex) {
    vec3 lightDir = normalize(-light.direction);
    vec3 halfwayDir = normalize(-light.direction + viewDir);
    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess);
    vec3 ambient = light.ambient * tex;
    vec3 diffuse = light.diffuse * diff * tex;
    vec3 specular = light.specular * spec * tex;
    return (ambient + diffuse + specular);
}

vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 tex) {
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(light.position + viewDir);
    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess);
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance*distance));
    vec3 ambient = light.ambient * tex;
    vec3 diffuse = light.diffuse * diff * tex;
    vec3 specular = light.specular * spec * tex;
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 tex) {
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 halfwayDir = normalize(light.position + viewDir);
    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess);
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance*distance));
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    vec3 ambient = light.ambient * tex;
    vec3 diffuse = light.diffuse * diff * tex;
    vec3 specular = light.specular * spec * tex;
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}
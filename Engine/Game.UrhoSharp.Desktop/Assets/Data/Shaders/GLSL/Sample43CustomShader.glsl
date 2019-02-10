#include "Uniforms.glsl"
#include "Samplers.glsl"
#include "Transform.glsl"
#include "ScreenPos.glsl"
#include "Fog.glsl"

varying vec2 vTexCoord;
varying vec4 vWorldPos;
varying vec3 vNormal;
varying vec4 vEyeVec;


void VS()
{
    mat4 modelMatrix = iModelMatrix;
    vNormal = GetWorldNormal(iModelMatrix);
    vec3 worldPos = GetWorldPos(modelMatrix);
    gl_Position = GetClipPos(worldPos);
    vTexCoord = GetTexCoord(iTexCoord);
    vWorldPos = vec4(worldPos, GetDepth(gl_Position));
    vEyeVec = vec4(cCameraPos - worldPos, GetDepth(gl_Position));
}

void PS()
{
    vec3 normal = normalize(vNormal);
    vec3 camera = normalize(vEyeVec.xyz);

    float f = dot(camera, normal);
    gl_FragColor = vec4(1, 1, 1, 1 - f);
}

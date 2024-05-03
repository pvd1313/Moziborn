#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

TEXTURE2D(_WarFogMapGroundTexture);
SAMPLER(sampler_WarFogMapGroundTexture);
TEXTURE2D(_WarFogMapRoofTexture);
SAMPLER(sampler_WarFogMapRoofTexture);
float _WarFogGroundRoofBlending;

float2 _AmbientLightMapWorldSize;
float3 _AmbientLightMapOffset;
TEXTURE2D(_AmbientLightMapTexture);
SAMPLER(sampler_AmbientLightMapTexture);

float3 _PointLightMapOffset;
float2 _PointLightMapWorldSize;
float2 _PointLightMapRotation;
TEXTURE2D(_PointLightMapTexture);
SAMPLER(sampler_PointLightMapTexture);

float _VerticalCutAlpha;
float4 _VerticalCutRange;
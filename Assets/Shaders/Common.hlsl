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

struct AmbientMapColorUV
{
    float color;
    float2 uv;
};

float Dither(float2 positionCS, float alpha)
{
    // positionCS *= _ScreenParams.xy;
    
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };

    int index = int(positionCS.x) % 4 * 4 + int(positionCS.y) % 4;
    
    return alpha - DITHER_THRESHOLDS[index];
}
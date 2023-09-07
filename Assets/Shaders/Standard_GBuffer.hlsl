#ifndef UNIVERSAL_LIT_GBUFFER_PASS_INCLUDED
#define UNIVERSAL_LIT_GBUFFER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

float2 _AmbientLightMapWorldSize;
float3 _AmbientLightMapOffset;
TEXTURE2D(_AmbientLightMapTexture);
SAMPLER(sampler_AmbientLightMapTexture);

TEXTURE2D(_WarFogMapTexture);
SAMPLER(sampler_WarFogMapTexture);

float3 _TorchLightMapOffset;
float2 _TorchLightMapWorldSize;
float2 _TorchLightMapRotation;
TEXTURE2D(_TorchLightMapTexture);
SAMPLER(sampler_TorchLightMapTexture);

float _VerticalCutAlpha;
float4 _VerticalCutRange;


// TODO: Currently we support viewDirTS caclulated in vertex shader and in fragments shader.
// As both solutions have their advantages and disadvantages (etc. shader target 2.0 has only 8 interpolators).
// We need to find out if we can stick to one solution, which we needs testing.
// So keeping this until I get manaul QA pass.
#if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitForwardPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD2;
#endif

    float3 normalWS                 : TEXCOORD3;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
#endif
    float3 viewDirWS                : TEXCOORD5;

    half3 vertexLighting            : TEXCOORD6;    // xyz: vertex lighting

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    float3 viewDirTS                : TEXCOORD8;
#endif
    
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = 0.0; // we don't apply fog in the guffer pass
    inputData.vertexLighting = input.vertexLighting.xyz;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitGBufferPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // Scale Z based on camera angle
    float3 objectCenterWS = TransformObjectToWorld(float3(0, 0, 0));
    float3 vertexWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 delta = vertexWS - objectCenterWS;
    float3 resultOS = TransformWorldToObject(objectCenterWS + delta);
    input.positionOS = float4(resultOS.x, resultOS.y, resultOS.z, input.positionOS.w);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.vertexLighting = vertexLight;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    return output;
}

FragmentOutput LitGBufferPassFragment(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
#endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

#ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
#endif
    
    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    float3 emission = GlobalIllumination(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.normalWS, inputData.viewDirectionWS);
    
    // Ambient map
    float3 positionWS = input.positionWS;
    float2 ambientMapUV = (positionWS + _AmbientLightMapOffset).xz / _AmbientLightMapWorldSize;
    float ambientMapColor = SAMPLE_TEXTURE2D(_AmbientLightMapTexture, sampler_AmbientLightMapTexture, ambientMapUV);

    // Torch light
    float2 torchMapPos = (positionWS + _TorchLightMapOffset).xz;
    
    torchMapPos = float2(torchMapPos.x * _TorchLightMapRotation.x - torchMapPos.y * _TorchLightMapRotation.y,
                         torchMapPos.x * _TorchLightMapRotation.y + torchMapPos.y * _TorchLightMapRotation.x);
    
    float2 torchMapUV = saturate(torchMapPos / _TorchLightMapWorldSize);
    float torchMapColor = SAMPLE_TEXTURE2D(_TorchLightMapTexture, sampler_TorchLightMapTexture, torchMapUV);

    float torchViewDot = 0.8 + 0.2 * saturate(dot(input.normalWS, input.viewDirWS));
    float maxHeight = 2 * pow(torchMapColor, 0.2);
    float torchHeight01 = 1 - saturate(positionWS.y / maxHeight);
    torchMapColor *= torchHeight01 * torchViewDot;

    // Cut walls
    float verticalCutDelta = _VerticalCutRange.y - _VerticalCutRange.x;
    clip(-input.positionWS.y + _VerticalCutRange.x + verticalCutDelta * _VerticalCutAlpha + 0.00001);

    emission = emission * ambientMapColor + brdfData.albedo * torchMapColor;
    
    // War fog
    float warFogColor = SAMPLE_TEXTURE2D(_WarFogMapTexture, sampler_WarFogMapTexture, ambientMapUV);
    
    //float3 grayAlbedo = dot(brdfData.albedo, float3(0.3, 0.59, 0.11));
    //brdfData.albedo = lerp(brdfData.albedo, grayAlbedo, 1 - warFogColor);
    brdfData.albedo *= warFogColor;

    //float3 grayEmission = dot(emission, float3(0.3, 0.59, 0.11));
    //emission = lerp(emission, grayEmission, 1 - warFogColor);
    emission *= warFogColor;
    
#if DEBUG_WARFOG
     brdfData.albedo = 0;
     emission = warFogColor;
#endif

#if DEBUG_AMBIENT
    brdfData.albedo = 0;
    emission = ambientMapColor;
#endif

#if DEBUG_TORCH
    brdfData.albedo = 0;
    emission = torchMapColor;
#endif
    
    return BRDFDataToGbuffer(brdfData, inputData, surfaceData.smoothness, surfaceData.emission + emission);
}

#endif

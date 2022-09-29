Shader "ProjectTolerance/BuildingStructure"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _VerticalCutAlpha("VerticalCutAlpha", Range(0, 1)) = 1
        _VerticalCutRange("VerticalCutRange", Vector) = (0, 0, 0, 0)
        _Emission ("Emission", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Custom fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };

        // local
        float _Glossiness;
        float _Metallic;
        float4 _Color;
        float4 _Emission;
        float _VerticalCutAlpha;
        float4 _VerticalCutRange;

        // global
        float3 _AmbientLightMapOffset;
        sampler2D _AmbientLightMapTexture;
        float2 _AmbientLightMapSize;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        inline void LightingCustom_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
        {
            LightingStandard_GI(s, data, gi);
        }

        inline half4 LightingCustom_Deferred(SurfaceOutputStandard s, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
        {
            gi.indirect.diffuse *= s.Alpha;
            
            return LightingStandard_Deferred(s, 0, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _Emission;

            float2 ambientUV = (IN.worldPos + _AmbientLightMapOffset).xz / _AmbientLightMapSize;
            o.Alpha = tex2D(_AmbientLightMapTexture, ambientUV);

            float verticalCutDelta = _VerticalCutRange.y - _VerticalCutRange.x;
            clip(-IN.worldPos.y + _VerticalCutRange.x + verticalCutDelta * _VerticalCutAlpha + 0.00001);
        }
        ENDCG
    }
    FallBack "Diffuse"
}

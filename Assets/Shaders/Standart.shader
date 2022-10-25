Shader "ProjectTolerance/Standart"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
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
        #pragma surface surf Custom vertex:vert

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
        float4 _Color;
        float4 _Emission;
        float _VerticalCutAlpha;
        float4 _VerticalCutRange;

        // global
        float3 _AmbientLightMapOffset;
        sampler2D _AmbientLightMapTexture;
        float2 _AmbientLightMapSize;
        
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

        void vert (inout appdata_full v)
        {
            float4 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1.0));
            float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
            float4 delta = vertexPos - objectCenter;
            delta.z *= 1.064178;

            v.vertex = mul(unity_WorldToObject, objectCenter + delta);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Emission = _Emission;

            float3 worldPos = IN.worldPos;
            worldPos.z *= 0.939693;
            
            float2 ambientUV = (worldPos + _AmbientLightMapOffset).xz / _AmbientLightMapSize;
            o.Alpha = tex2D(_AmbientLightMapTexture, ambientUV);

            float verticalCutDelta = _VerticalCutRange.y - _VerticalCutRange.x;
            
            clip(-IN.worldPos.y + _VerticalCutRange.x + verticalCutDelta * _VerticalCutAlpha + 0.00001);
            
        }
        ENDCG
        
        Pass 
        {
            Name "ShadowCaster"

            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            struct v2f
            { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;

                float4 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1.0));
                float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
                float4 delta = vertexPos - objectCenter;
                delta.z *= 1.064178;

                v.vertex = mul(unity_WorldToObject, objectCenter + delta);
                
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                
                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                SHADOW_CASTER_FRAGMENT(i)
            }

            ENDCG
        }
    }
}

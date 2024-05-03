Shader "Moziborn/AimLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color0 ("Color0", Color) = (1, 1, 1, 1)
        _ColorDither("ColorDither", float) = 1
        _Color1 ("Color1", Color) = (1, 1, 1, 1)
        _ColorBlend ("ColorBlend", float) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            
            #include "Common.hlsl"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL; 
            };

            struct FragmentInput
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float viewDirWS : TEXCOORD2;
            };

            float4 _Color0;
            float4 _Color1;
            float _ColorBlend;
            float _ColorDither;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.positionWS = mul(unity_ObjectToWorld, input.positionOS);
                output.normalWS = mul(unity_ObjectToWorld, input.normalOS);
                output.viewDirWS = UNITY_MATRIX_IT_MV[2].xyz;
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                clip(Dither(input.positionCS, _ColorDither));
                
                float colorChoice = Dither(input.positionCS, _ColorBlend * _ColorDither);
                float4 color;
                
                if (colorChoice < 0)
                {
                    color = _Color0;
                }
                else
                {
                    color = _Color1;
                }

                // Ambient Light
                float2 ambientMapUV = (input.positionWS + _AmbientLightMapOffset).xz / _AmbientLightMapWorldSize;
                float ambientMapColor = SAMPLE_TEXTURE2D(_AmbientLightMapTexture, sampler_AmbientLightMapTexture, ambientMapUV);

                // Torch light
                float2 torchMapPos = (input.positionWS + _PointLightMapOffset).xz;
    
                torchMapPos = float2(torchMapPos.x * _PointLightMapRotation.x - torchMapPos.y * _PointLightMapRotation.y,
                                     torchMapPos.x * _PointLightMapRotation.y + torchMapPos.y * _PointLightMapRotation.x);
    
                float2 pointLightMapUV = saturate(torchMapPos / _PointLightMapWorldSize);
                float pointLightMapColor = SAMPLE_TEXTURE2D(_PointLightMapTexture, sampler_PointLightMapTexture, pointLightMapUV);

                float pointLightViewDot = 0.8 + 0.2 * saturate(dot(input.normalWS, input.viewDirWS));
                float maxHeight = 2 * pow(pointLightMapColor, 0.2);
                float pointLightHeight01 = 1 - saturate(input.positionWS.y / maxHeight);
                pointLightMapColor *= pointLightHeight01 * pointLightViewDot;

                color.a *= max(pow(pointLightMapColor, 0.5), pow(ambientMapColor, 1.25));

                return color;
            }
            ENDCG
        }
    }
}
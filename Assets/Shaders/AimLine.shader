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

            #include "UnityCG.cginc"
            #include "Common.hlsl"

            struct VertexInput
            {
                float4 vertexOS : POSITION;
            };

            struct FragmentInput
            {
                float4 vertexCS : SV_POSITION;
                float4 vertexOS : TEXCOORD0;
            };

            float4 _Color0;
            float4 _Color1;
            float _ColorBlend;
            float _ColorDither;

            FragmentInput vertex(VertexInput input)
            {
                FragmentInput output;
                
                output.vertexCS = UnityObjectToClipPos(input.vertexOS);
                output.vertexOS = input.vertexOS;
                
                return output;
            }

            float4 fragment(FragmentInput input) : SV_Target
            {
                clip(Dither(input.vertexCS, _ColorDither));
                
                float colorChoice = Dither(input.vertexCS, _ColorBlend * _ColorDither);
                float4 color;
                
                if (colorChoice < 0)
                {
                    color = _Color0;
                }
                else
                {
                    color = _Color1;
                }

                //color.a *= pow(1 - abs(input.vertexOS.x * 2), 0.25);
                

                return color;
            }
            ENDCG
        }
    }
}
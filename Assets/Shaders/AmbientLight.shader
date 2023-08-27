Shader "Hidden/AmbientLight"
{
    Properties
    {
        _WallsTexture ("Texture", 2D) = "white" {}
        _SunLightTexture ("Texture", 2D) = "white" {}
        _StateTexture ("Texture", 2D) = "black" {}
        _TextureSize("TextureSize", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            FragmentInput Vertex (VertexInput input)
            {
                FragmentInput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                
                return output;
            }

            sampler2D _WallsTexture;
            sampler2D _SunLightTexture;
            sampler2D _StateTexture;
            float2 _TextureSize;

            fixed4 Fragment (FragmentInput i) : SV_Target
            {
                const float falloff = 0.03;
                
                float2 pixel = 1 / _TextureSize;
                
                float4 col1 = tex2D(_StateTexture, i.uv + float2(pixel.x, 0));
                float4 col2 = tex2D(_StateTexture, i.uv - float2(pixel.x, 0));
                float4 col3 = tex2D(_StateTexture, i.uv + float2(0, pixel.y));
                float4 col4 = tex2D(_StateTexture, i.uv - float2(0, pixel.y));
                float4 maxCol = max(max(col1, col2), max(col3, col4));
                float4 avgCol = (col1 + col2 + col3 + col4) * 0.25;

                float4 col = avgCol * (1 - falloff) + maxCol * falloff;
                
                col = saturate(col + tex2D(_SunLightTexture, i.uv));

                col *= 1 - tex2D(_WallsTexture, i.uv);
                
                return col;
            }
            ENDCG
        }
    }
}

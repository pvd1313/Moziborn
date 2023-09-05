Shader "ProjectTolerance/AmbientLight"
{
    Properties
    {
        _WallTexture ("Texture", 2D) = "white" {}
        _SunTexture ("Texture", 2D) = "white" {}
        _StateTexture ("Texture", 2D) = "black" {}
        _BilinearFall ("_BilinearFall", float) = 0.5
        _LinearFall ("_BilinearFall", float) = 0
        _Blur("Blur", float) = 0.5
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

            sampler2D _WallTexture;
            sampler2D _SunTexture;
            sampler2D _StateTexture;
            float4 _StateTexture_TexelSize;
            float _BilinearFall;
            float _LinearFall;
            float _Blur;

            fixed4 Fragment (FragmentInput i) : SV_Target
            {
                float2 pixel = _StateTexture_TexelSize.xy;
                
                float col1 = tex2D(_StateTexture, i.uv + float2(pixel.x, 0));
                float col2 = tex2D(_StateTexture, i.uv - float2(pixel.x, 0));
                float col3 = tex2D(_StateTexture, i.uv + float2(0, pixel.y));
                float col4 = tex2D(_StateTexture, i.uv - float2(0, pixel.y));
                float maxCol = max(max(col1, col2), max(col3, col4));
                float avgCol = (col1 + col2 + col3 + col4) * 0.25;

                float col = maxCol * _BilinearFall * (1 - _Blur) + avgCol * _Blur;
                col = saturate(col - _LinearFall);
                
                col = saturate(col + tex2D(_SunTexture, i.uv));

                col *= 1 - tex2D(_WallTexture, i.uv);
                
                return col;
            }
            
            ENDCG
        }
    }
}

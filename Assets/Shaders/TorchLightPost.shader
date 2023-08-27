Shader "Hidden/TorchLightPost"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_TextureSize("TextureSize", Vector) = (1, 1, 0, 0)
		_BlurSize("_BlurPixelSize", float) = 16
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
			
			sampler2D _MainTex;
			float2 _TextureSize;
			float _BlurPixelSize;

			float4 Fragment (FragmentInput i) : SV_Target
			{
				float2 pixel = 1 / _TextureSize * _BlurPixelSize;

				float4 col1 = tex2D(_MainTex, i.uv + float2(pixel.x, 0));
                float4 col2 = tex2D(_MainTex, i.uv - float2(pixel.x, 0));
                float4 col3 = tex2D(_MainTex, i.uv + float2(0, pixel.y));
                float4 col4 = tex2D(_MainTex, i.uv - float2(0, pixel.y));
				float4 avgCol = (col1 + col2 + col3 + col4) * 0.25;
				
				return avgCol;
			}
			ENDCG
		}
	}
}
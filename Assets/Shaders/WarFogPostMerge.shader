Shader "Moziborn/WarFogPostMerge"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" { }		
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
			float4 _MainTex_TexelSize;

			float4 Fragment (FragmentInput input) : SV_Target
			{
				float2 pixel = _MainTex_TexelSize.xy * 0.5;

				float4 color0 = tex2D(_MainTex, input.uv + float2(pixel.x, 0));
                float4 color1 = tex2D(_MainTex, input.uv - float2(pixel.x, 0));
                float4 color2 = tex2D(_MainTex, input.uv + float2(0, pixel.y));
                float4 color3 = tex2D(_MainTex, input.uv - float2(0, pixel.y));
                float4 color4 = tex2D(_MainTex, input.uv + float2(+pixel.x, -pixel.y));
                float4 color5 = tex2D(_MainTex, input.uv + float2(+pixel.x, +pixel.y));
                float4 color6 = tex2D(_MainTex, input.uv + float2(-pixel.x, -pixel.y));
                float4 color7 = tex2D(_MainTex, input.uv + float2(-pixel.x, +pixel.y));

				float finalColor = max(max(color0, color1), max(color2, color3));
				finalColor = max(finalColor, max(max(color4, color5), max(color6, color7)));
				
				return finalColor;
			}
			
			ENDCG
		}
	}
}
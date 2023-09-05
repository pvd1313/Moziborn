Shader "ProjectTolerance/WarFogPost"
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
			float _PixelOffset;

			float4 Fragment (FragmentInput i) : SV_Target
			{
				//float2 pixel = _MainTex_TexelSize.xy * 4;

				// float4 color0 = tex2D(_MainTex, i.uv + float2(pixel.x, 0));
				// float4 color1 = tex2D(_MainTex, i.uv - float2(pixel.x, 0));
                // float4 color2 = tex2D(_MainTex, i.uv + float2(0, pixel.y));
                // float4 color3 = tex2D(_MainTex, i.uv - float2(0, pixel.y));
				//
				// float4 color = (color0 + color1 + color2 + color3) * 0.1;
				//
				// return dot(color.rgb, float3(0.3, 0.59, 0.11)) * 0.1;

				return tex2D(_MainTex, i.uv);
			}
			
			ENDCG
		}
	}
}
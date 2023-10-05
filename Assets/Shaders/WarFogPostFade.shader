Shader "Moziborn/WarFogPostFade"
{
	Properties
	{
		_SunTexture ("Main Texture", 2D) = "white" { }	
		_StateTexture("State Texture", 2D) = "white" { }	
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
			
			sampler2D _SunTexture;
			sampler2D _StateTexture;

			float4 Fragment (FragmentInput input) : SV_Target
			{
				float4 targetColor = pow(tex2D(_SunTexture, input.uv), 0.1);
                float4 curColor = tex2D(_StateTexture, input.uv);
				
				return curColor + clamp(targetColor - curColor, -0.08, 0.08);
			}
			
			ENDCG
		}
	}
}
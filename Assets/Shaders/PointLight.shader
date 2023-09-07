Shader "ProjectTolerance/PointLight"
{
	Properties
	{
		_SpreadAngleRad("Spread angle", float) = 90
		_MaxRayDistance("Ray distance", float) = 5
		_StartOffset("Start offset", float) = 0.3
		
		_Intensity("Intensity", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		
		Blend One One
		
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment

			#pragma multi_compile_local START_OFFSET_ON START_OFSET_OFF
			
			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 vertexOS : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct FragmentInput
			{
				float3 vertexOS : TEXCOORD1;
				float2 uv : TEXCOORD0;
				float4 vertexSS : SV_POSITION;
			};

			#define HIT_PENETRATION_DISTANCE 0.03

			float _SpreadAngleRad;
			float _MaxRayDistance;
			float _Intensity;
			float _StartOffset;
			
			FragmentInput Vertex (VertexInput input)
			{
				FragmentInput output;
				
				float hitDistance01 = input.uv.x;
				float3 vertexOS = input.vertexOS * hitDistance01 * _MaxRayDistance;
				vertexOS += input.vertexOS * HIT_PENETRATION_DISTANCE;

				output.vertexOS = vertexOS;
				output.vertexSS = UnityObjectToClipPos(vertexOS);
				output.uv = input.vertexOS.xz * 0.5 * hitDistance01 + float2(0.5, 0.5);

				return output;
			}
			
			float Fragment (FragmentInput input) : SV_Target
			{
				float angle = abs(atan2(input.vertexOS.x, input.vertexOS.z));
				float spread = pow(1 - pow(angle / _SpreadAngleRad * 2, 1.5), 2);
				float distance = 1 - length(input.vertexOS.xz) / (_MaxRayDistance + HIT_PENETRATION_DISTANCE);
				float offset = saturate(saturate(length(input.vertexOS.xz) - _StartOffset) / 0.01);
				
				return spread * pow(distance, 2) * _Intensity * offset;
			}
			
			ENDCG
		}
	}
}
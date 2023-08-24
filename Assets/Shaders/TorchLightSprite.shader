Shader "ProjectTolerance/TorchLightSprite"
{
	Properties
	{
		_SpreadAngleRad("Spread angle", float) = 90
		_MaxRayDistance("Ray distance", float) = 5
		
		_Intensity("Intensity", float) = 1
		_CenterReduction("Center reduction", float) = 0.2
		_zSpreadPower("Z spread", float) = 1
		_zDistancePower("Z distance", float) = 2
		_ySpreadPower("Y spread", float) = 0.3
		_yDistancePower("Y distance", float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment
			
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

			float _SpreadAngleRad;
			float _MaxRayDistance;
			float _CenterReduction;
			float _zSpreadPower;
			float _zDistancePower;
			float _ySpreadPower;
			float _yDistancePower;
			float _Intensity;
			
			FragmentInput Vertex (VertexInput input)
			{
				const float HitPenetrationDistance = 0.03;
				
				FragmentInput output;
				
				float hitDistance01 = input.uv.x;
				float3 vertexOS = input.vertexOS * hitDistance01 * _MaxRayDistance;
				vertexOS += input.vertexOS * HitPenetrationDistance;

				output.vertexOS = vertexOS;
				output.vertexSS = UnityObjectToClipPos(vertexOS);
				output.uv = input.vertexOS.xz * 0.5 * hitDistance01 + float2(0.5, 0.5);
				
				return output;
			}
			
			float2 Fragment (FragmentInput input) : SV_Target
			{
				// Basic, z-axis, distance and spread fall
				float angle = atan2(input.vertexOS.x, input.vertexOS.z);
				float zSpread01 = 1 - max(_CenterReduction, abs(angle / _SpreadAngleRad * 2));
				float zDistance01 = 1 - length(input.vertexOS.xz) / _MaxRayDistance;
				zDistance01 = pow(zDistance01, _zDistancePower);
				zSpread01 = pow(zSpread01, _zSpreadPower);

				// Circle shape on walls, y-axis
				float yDistance01 = length(input.vertexOS.xz) / _MaxRayDistance;
				float ySpread01 = 1 - max(_CenterReduction, abs(angle / _SpreadAngleRad * 2));
				ySpread01 = pow(ySpread01, _ySpreadPower);
				yDistance01 = pow(yDistance01, _yDistancePower);
				
				return float2(zSpread01 * zDistance01 * _Intensity, ySpread01 * yDistance01);
			}
			
			ENDCG
		}
	}
}
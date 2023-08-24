Shader "ProjectTolerance/TorchLightSprite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		[HideInInspector] _SpreadAngleRad("Spread angle", float) = 90
		[HideInInspector] _MaxRayDistance("Ray distance", float) = 5
		_TiltCenterReduction("Tilt center reduction", float) = 0.2
		_TiltParallelReduction("Tilt parallel reduction", float) = 0.2
		_TiltOriginReduction("Tilt origin reduction", float) = 2
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
			
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 vertexOS : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct FragmentInput
			{
				float3 vertexOS : COLOR1;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertexSS : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _SpreadAngleRad;
			float _MaxRayDistance;
			float _TiltCenterReduction;
			float _TiltParallelReduction;
			float _TiltOriginReduction;
			
			FragmentInput Vertex (VertexInput input)
			{
				const float HitPenetrationDistance = 0.03;
				
				FragmentInput output;
				
				float hitDistance01 = input.uv.x;
				float3 vertexOS = input.vertexOS * hitDistance01 * _MaxRayDistance;
				vertexOS += input.vertexOS * HitPenetrationDistance;

				output.vertexOS = vertexOS;
				output.vertexSS = UnityObjectToClipPos(vertexOS);
				output.uv = float2(0.5, 0.5) + input.vertexOS.xz * 0.5 * hitDistance01;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return output;
			}
			
			float Fragment (FragmentInput input) : SV_Target
			{
				float PI = 3.1415926;
				
				float4 col = tex2D(_MainTex, input.uv);
				
				UNITY_APPLY_FOG(i.fogCoord, col);

				float tilt = 1 - max(_TiltCenterReduction, abs(atan2(input.vertexOS.x, input.vertexOS.z) / (_SpreadAngleRad * 0.5)));
				
				float perimeter = 2 * PI * length(input.vertexOS.xz);
				float angle = atan2(input.vertexOS.x, input.vertexOS.z);

				float fullP = 2 * PI * _MaxRayDistance * _SpreadAngleRad;
				
				tilt -= perimeter * abs(angle) / fullP * _TiltParallelReduction;

				tilt *= pow(length(input.vertexOS.xz) / _MaxRayDistance, _TiltOriginReduction);
				
				return tilt * col;
			}
			
			ENDCG
		}
	}
}
Shader "ProjectTolerance/TorchLightSprite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_SpreadAngleRad("Spread angle", float) = 90
		_MaxRayDistance("Ray distance", float) = 5
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
				float4 vertexOS : COLOR1;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertexSS : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _SpreadAngleRad;
			float _MaxRayDistance;
			
			FragmentInput Vertex (VertexInput input)
			{
				const float HitPenetrationDistance = 0.03;
				
				FragmentInput output;
				
				float hitDistance01 = (input.color.r * 65535 + input.color.g * 256) / 65535;
				float3 vertexOS = input.vertexOS * hitDistance01 * _MaxRayDistance;
				vertexOS += input.vertexOS * HitPenetrationDistance;

				output.vertexOS = input.vertexOS;
				output.vertexSS = UnityObjectToClipPos(vertexOS);
				output.uv = float2(0.5, 0.5) + input.vertexOS.xz * 0.5 * hitDistance01;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return output;
			}
			
			float Fragment (FragmentInput input) : SV_Target
			{
				float4 col = tex2D(_MainTex, input.uv);
				
				UNITY_APPLY_FOG(i.fogCoord, col);

				float tilt = 1 - max(0.2, abs(atan2(input.vertexOS.x, input.vertexOS.z) / (_SpreadAngleRad * 0.5)));
				
				return col * tilt;
			}
			
			ENDCG
		}
	}
}
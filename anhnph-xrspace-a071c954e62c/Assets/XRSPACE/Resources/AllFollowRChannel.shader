// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AllFollowRChannel"
{
	Properties{ _MainTex("Texture", any) = "" {} }

		SubShader{

			Tags { "ForceSupported" = "True" }

			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			Fog { Mode Off }
			ZTest Always

			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;

				uniform float4 _MainTex_ST;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 o_color = 2.0 * tex2D(_MainTex, i.texcoord) * i.color;
					o_color.g = o_color.r;
					o_color.b = o_color.r;
					o_color *= 0.6;
					o_color.a = 1;
					return o_color;
				}
				ENDCG
			}
	}

		Fallback off
}

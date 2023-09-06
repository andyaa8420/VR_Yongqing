Shader "_Bison/BeamRenderer/DistantPointer2.5"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", COLOR) = (1,1,1,1)
		_Value("Value", Range(10,30)) = 20
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "Queue" = "Overlay+100" }
			LOD 100

			//Blend SrcColor One
			//Blend One One
			//Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcColor
			//Blend SrcFactor DstFactor
			Blend SrcAlpha OneMinusSrcAlpha

			Zwrite off
			Ztest always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
				};

				sampler2D _MainTex;
				//float4 _MainTex_ST;
				fixed4 _Color;
				fixed _Value;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv; // TRANSFORM_TEX(v.uv, _MainTex);
					o.color = v.color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col;

					fixed4 tex = tex2D(_MainTex, i.uv);

					half fc0 = saturate((1.0 - i.uv.x) * 60);
					half fc1 = saturate(i.uv.x * 4);

					//..........................
					half u = (1.0 - i.uv.x) * _Value;
					half2 uv = i.uv;

					uv.x = u;
					fixed alpha = tex2D(_MainTex, uv).a;



					//col.rgb = (tex.r * _Color.rgb) + tex.g;
					col.rgb = (tex.r * i.color) + tex.g;
					col.a = tex.b * fc0 * fc1 * _Color.a * alpha;

					return col;
				}
				ENDCG
			}
		}
}

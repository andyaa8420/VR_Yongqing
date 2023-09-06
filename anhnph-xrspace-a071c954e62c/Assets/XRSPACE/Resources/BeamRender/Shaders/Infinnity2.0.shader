Shader "_Bison/BeamRenderer/Infinnity2.0"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
		//_Color("Color", COLOR) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Overlay+100" }
		LOD 100

		//Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha
		Zwrite off

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

			//sampler2D _MainTex;
			//float4 _MainTex_ST;
			//fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv; // TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
			
				col.rgb = i.color.rgb;
				col.a = saturate((1.0 - i.uv.y)*2.0 * i.uv.y*2.0) * i.color.a;

				return col;
			}
			ENDCG
		}
	}
}

Shader "Custom/ImageOverlay"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha // use alpha blending
			ZTest Always // deactivate depth test

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag

			#include "UnityCG.cginc" 

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Pos;
			float4x4 _Current_MVP;

			struct appdata
			{
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.pos = mul(_Current_MVP, mul(unity_ObjectToWorld, v.pos));

				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 color = tex2D(_MainTex, i.uv);

				return color;
			}

			ENDCG
		}
	}
		FallBack "Diffuse"
}

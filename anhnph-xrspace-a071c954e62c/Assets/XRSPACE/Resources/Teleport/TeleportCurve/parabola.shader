Shader "_Bison/UI/parabola"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_tex1("Texture 1", 2D) = "white" {}
		_Color("Color", COLOR) = (1,1,1,1)
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
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _tex1;
			half4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = o.uv1 = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1.x -= _Time.x*10.0;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;

				//.......................................
				fixed tex2 = tex2D(_tex1, i.uv1).r;

				col.rgb = i.color.rgb * _Color;
				
				col.a = tex2D(_MainTex, i.uv).r * i.color.a + tex2*0.3;

				return col;
			}
			ENDCG
		}
	}
}

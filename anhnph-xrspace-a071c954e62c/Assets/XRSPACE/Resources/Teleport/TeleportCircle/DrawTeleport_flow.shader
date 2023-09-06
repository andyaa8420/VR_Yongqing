Shader "_Bison/DrawTeleport_flow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Visibility("Visibility", Range(0,1)) = 1
		_FlowSpeed("FlowSpeed", Range(1, 8)) = 2
	}
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100

		Blend One One

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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos: TEXCOORD1;
				float t: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			fixed4 _Color;
			float _FlowSpeed;
			float _Visibility;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.t = _Time.y*_FlowSpeed;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_MainTex, i.uv) * _Visibility;

				fixed tt = 0.5 + 0.5 + 0.3;

				fixed d = lerp(0, 1, sin(i.worldPos.y*20.0-i.t));

				col.rgb *= 0.6 + d*0.4;
				//col.rgb *= 2.5;

                return col;
            }
            ENDCG
        }
    }
}

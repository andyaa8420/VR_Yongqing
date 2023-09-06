Shader "_Bison/BeamRenderer/Dot2.0"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tex2("Texture2", 2D) = "white"{}
        _Color("Color", Color) = (1,1,1,1)
        _Blend("Blend",Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Transparent"}
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha 
		Cull off
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Tex2;
            fixed4 _Color;
            float _Blend;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col;
                // sample the texture
                fixed4 col1 = tex2D(_MainTex, i.uv) * _Color;
                fixed4 col2 = tex2D(_Tex2, i.uv) * _Color;

                col = lerp(col1, col2, _Blend);

                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/TextureMask"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Mask("Culling Mask", 2D) = "white" {}
	}
		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}
			Cull Back
			Lighting Off
			ZWrite Off
			ZTest Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				/*SetTexture[_Mask] {combine texture}
				SetTexture[_MainTex] {combine texture, previous}*/

				CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc" 

				uniform sampler2D _MainTex;
				uniform sampler2D _Mask;
				float4 _MainTex_ST;

				fixed4 frag(v2f_img i) : SV_Target
				{
					half4 color = tex2D(_MainTex, i.uv);
					half4 maskCol = tex2D(_Mask, i.uv);
					color *= maskCol;

					return color;
				}

				ENDCG
			}

		}
}

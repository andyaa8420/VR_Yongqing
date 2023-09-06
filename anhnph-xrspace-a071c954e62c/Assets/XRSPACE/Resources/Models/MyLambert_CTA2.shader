Shader "_My/MyLambert_CTA2"
{
	Properties
	{
		[NoScaleOffset] _texColor("Color (RGB)", 2D) = "white" {}
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}

		_texAO("AO (RGB)", 2D) = "white" {}

		_AOValue("AO Value", range(0,1)) = 0.8
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert

		sampler2D	_texColor;
		sampler2D	_MainTex;
		sampler2D	_texAO;

		half _Shininess;

		half _AOValue;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_texColor;
			float2 uv2_texAO;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)


		void surf(Input IN, inout SurfaceOutput output)
		{
			fixed4 texColor = tex2D(_texColor, IN.uv_texColor);
			fixed texAO = tex2D(_texAO, IN.uv2_texAO);
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

			output.Albedo = tex.rgb * texColor.rgb * (texAO * _AOValue + (1.0 - _AOValue));

			output.Alpha = 1;
		}
		ENDCG
	}
	Fallback "VertexLit"
}



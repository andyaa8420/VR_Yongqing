Shader "_Bison/XRHand"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5, 0.5, 0.5, 1.0)
		_Transparency("Transparency",  Range(0.0, 1.0))  = 1.0

		[Space(20)]
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
		_Outline("Outline width", Range(0.0, 1.0)) = .3
		_OutlineValue("_OutlineValue", Range(0,3)) = 1.0

		[Space(20)]
		_InnerColor("Inner Color", Color) = (0.0, 0.5, 1.0, 1.0)
		_Thickness("Thickness", Range(0,1)) = 1
		_InnerValue("_InnerValue", Range(0,2)) = 0.5

		//[Space(20)]
		//_Value("Value", Range(0,1)) = 0.5
	}


	SubShader{
		Tags{ "Queue" = "Transparent" }

		Pass
		{
			Name "BASE"
			Cull Back
			Blend Zero One
			//Offset -8, -8

			//ZWrite On
			//ColorMask 0

			SetTexture[_OutlineColor]{
				ConstantColor(0,0,0,0)
				Combine constant
			}

			//SetTexture[_OutlineColor]
			//{
			//	ConstantColor(0,0,0,0)
			//	Combine constant
			//}
		}
		//Pass
		//{
		//	ZWrite On
		//	ColorMask 0
		//}
		Pass
		{
			Name "OUTLINE"
			Tags{ "Queue" = "Transparent" }

			Cull Front
			//Cull Back
			//Cull off

			Blend One One
			//Blend One OneMinusDstColor

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				//float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
				//float2 texcoord : TEXCOORD0;
				//float3 worldPos: TEXCOORD1;
				//float3 screenPos: TEXCOORD1;
				//float2 ppos: TEXCOORD1;
			};

			//sampler2D _Fading;
			//float4 _Fading_ST;
			uniform float _Outline;
			uniform float4 _OutlineColor;
			float _OutlineValue;

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
				float2 offset = TransformViewToProjection(norm.xy);
				//o.texcoord = TRANSFORM_TEX(v.texcoord, _Fading);

				float clipSpaceRange01 = UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z);
				o.pos.xy += offset * _Outline * clipSpaceRange01 * 0.03;
				o.color = v.color;

				return o;
			}

			half4 frag(v2f i) :COLOR
			{
				fixed4 col = _OutlineColor * _OutlineValue * (1.0 - i.color.a);

				return col;
			}
			ENDCG
		}

		Pass
		{
			Name "INNER"
			Tags{ "Queue" = "Transparent" }

			//ZWrite Off
			//ZTest Equal
			//Cull Front
			Cull Back
			//Cull off

			//Blend One One
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite on

			//Offset -8, -8




			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				//float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex  : SV_POSITION;
				float4 color : COLOR;
				//float2 texcoord : TEXCOORD0;
				half4 projPos  : TEXCOORD1;
				half3 normal   : TEXCOORD2;
				half3 vDir     : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _Fading;
			float4 _Fading_ST;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.texcoord = TRANSFORM_TEX(v.texcoord, _Fading);
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);

				o.normal = v.normal;
				o.vDir = ObjSpaceViewDir(v.vertex);
				o.color = v.color;

				return o;
			}
			half4 _InnerColor;
			half4 _Color;
			float _Thickness;
			float _Transparency;
			//float _Value;
			half _InnerValue;

			fixed4 frag(v2f i) : SV_Target
			{
				i.normal = normalize(i.normal);
				i.vDir = normalize(i.vDir);
				half fresnel = dot(i.normal, i.vDir);
				half p = smoothstep(_Thickness*2.0, 0.0, abs(fresnel));

				//fixed4 fadingAlpha = tex2D(_Fading, i.texcoord);

				half4 col = _Color;
				col.rgb = _Color.rgb + _InnerColor.rgb * pow(p*_InnerValue,4.0);
				//col.a *= fadingAlpha.r;
				col.a *= (1.0 - i.color.a) * _Transparency;

				//col.rgb = 1.0-i.color.a;
				//col.a = 1;

				return col;
			}
			ENDCG
		}

	}
	Fallback "Diffuse"
}
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Shadow/Self_Illlumin_Diffuse"
{
	Properties{
		_MainColor("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}  

		_luminance("Luminance", Float) = 1

		_H("H", Float) = 0.09
		_ShadowColor("Shadow Color", Color) = (0, 0, 0, 0.4)
		_ShadowDir("_ShadowDir", Vector) = (0, -0.5, 1, 1)
	}
	SubShader{

	Pass { 

	Tags { "RenderType"="Opaque" }
	Lighting Off 
	
		CGPROGRAM
			#pragma vertex vert0
			#pragma fragment frag0
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f0 {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _MainColor;
			float _luminance;
			
			v2f0 vert0 (appdata_t v)
			{
				v2f0 o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			fixed4 frag0 (v2f0 i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord)*_MainColor * _luminance;
				return col;
			}
		ENDCG
	}

	Pass{
		 	Tags{ "Queue" = "Transparent=10" "IgnoreProjector" = "True" "RenderType" = "Transparent"} 

			Stencil{
			Ref 100
			Comp notequal
			Pass replace
			ReadMask 255
			WriteMask 255
			}

			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off
			ColorMask RGB
			ZWrite Off

			CGINCLUDE
	#include "UnityCG.cginc" 

			struct appdata {
			float4 vertex :
			POSITION;
		};

		struct v2f {
			float4 pos :
			POSITION;
			float a :
			TEXCOORD0;
		};

		float _H;
		float4 _ShadowDir;
		float4 _ShadowColor;

		v2f vert(appdata v)
		{
			v2f o;
			float4 v4 = mul(unity_ObjectToWorld, v.vertex);
			//v4.xyz = v4.w;
			v4.y -= _H;
			o.a = v4.y;

			float3 shadowDir = normalize(_ShadowDir.xyz);

			float3 v3 = v4.xyz + shadowDir * v4.y / dot(shadowDir, float3(0, -1, 0));
			v4.xyz = v3.xyz;
			v4.y = _H;
			o.pos = mul(UNITY_MATRIX_VP, v4);

			return o;
		}

		half4 frag(v2f i) : COLOR
		{
			return _ShadowColor * sign(i.a);
		}

			ENDCG

			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
			ENDCG
		}
	}
	
		FallBack "Self-Illumin/VertexLit"
}

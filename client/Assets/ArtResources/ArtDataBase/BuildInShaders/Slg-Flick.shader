// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SLG/Flick" {
Properties {
	_Color ("Main Color", Color) = (.5,.5,.5,1)
	_OutlineColor ("Outline Color", Color) = (1,1,1,1)
	_Outline ("Outline width", Range (.002, 0.1)) = .02
	_MainTex ("Base (RGB)", 2D) = "white" { }
}

CGINCLUDE
#include "UnityCG.cginc"

struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f {
	float4 pos : POSITION;
	float4 color : COLOR;
};

uniform float _Outline;
uniform float4 _OutlineColor;

v2f vert(appdata v) {
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);

	float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
	float2 offset = TransformViewToProjection(norm.xy);

	o.pos.xy += offset * _Outline;
	o.color = _OutlineColor;
	return o;
}
ENDCG

SubShader {
	Tags { "RenderType"="Opaque" }
	Pass {
  	ZWrite On
	  SetTexture [_MainTex]
		{
		  combine texture * primary
  	}
  }

	Pass {
		Tags { "LightMode" = "Always" }
		Cull Front
		ZWrite On
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		half4 frag(v2f i) :COLOR
		{
		  half4 c = i.color;
			c.a = abs(_SinTime.w);
		 return c;
		}
		ENDCG
	}
}
Fallback "Mobile/VertexLit"
}

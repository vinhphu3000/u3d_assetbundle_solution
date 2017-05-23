Shader "Alice/Particles/Alpha Add Tint Level ColorMask" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_AdditiveTex ("Additive Texture", 2D) = "black" {}
	_ColorMaskTex ("Color Mask Texture", 2D) = "white" {}
	_Level("Brightness", Float) = 1
}

CGINCLUDE

	#include "UnityCG.cginc"
	
	uniform sampler2D _MainTex;
	uniform sampler2D _ColorMaskTex;
	uniform sampler2D _AdditiveTex;
	uniform half4 _MainTex_ST;
	uniform half4 _ColorMaskTex_ST;
	uniform half4 _AdditiveTex_ST;
	half4 _TintColor;
	half _Level;

	struct appdata {
		float4 vertex : POSITION;
		half2 texcoord : TEXCOORD0;
		half4 color : COLOR0;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		half2	uv : TEXCOORD0;
		half4	uv2 : TEXCOORD1;

		half4	color : COLOR;
	};	

	v2f vert (appdata v)
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);		
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.uv2.xy = v.texcoord.xy * _ColorMaskTex_ST.xy + _ColorMaskTex_ST.zw;
		o.uv2.zw = v.texcoord.xy * _AdditiveTex_ST.xy + _AdditiveTex_ST.zw;
		
		o.color = v.color * _TintColor;
		return o;
	}
	
	half4 frag (v2f i) : COLOR
	{
		half4 clr = tex2D( _MainTex, i.uv ) * tex2D( _ColorMaskTex, i.uv2.xy );
		clr.rgb += tex2D( _AdditiveTex, i.uv2.zw ).rgb *clr.a;
		half4 texcol = clr * i.color * _Level;
		return texcol;
	}

	ENDCG
	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }

    Pass {

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	
	ENDCG
    }
}
Fallback Off
}
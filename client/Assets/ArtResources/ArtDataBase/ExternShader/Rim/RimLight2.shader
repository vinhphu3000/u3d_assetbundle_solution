// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "TGame/RimLight2" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ScreenTex ("Screen Tex", 2D) = "white" {}
        _ScreenTexScale ("ScreenTexScale", Float) = 3
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Float) = 0.7
        _ColorScale ("Color Scale", Float) = 1.4
    }
	
	Category
	{
		
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater .01
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off 
 
    SubShader {
        Pass { 
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag 
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 uv2 : TEXCOORD1;
                    fixed3 color : COLOR;
                };

                uniform float4 _MainTex_ST;
                uniform fixed4 _RimColor;
                float _RimWidth;

                v2f vert (appdata_base v) {
                    v2f o;
                    o.pos = UnityObjectToClipPos (v.vertex);

                    float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                    float dotProduct = 1 - dot(v.normal, viewDir);
                   
                    o.color = smoothstep(1 - _RimWidth, 1.0, dotProduct);
                    o.color *= _RimColor;

//                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv = v.texcoord.xy;
					o.uv2 = ComputeGrabScreenPos(o.pos);
					
                    return o;
                }

                uniform sampler2D _MainTex;
				uniform sampler2D _ScreenTex;
                uniform fixed4 _MainColor;
				uniform fixed _ScreenTexScale;
				uniform fixed _ColorScale;

                fixed4 frag(v2f i) : COLOR {
                    fixed4 texcol = tex2D(_MainTex, i.uv);
                    fixed4 texScreen = tex2D(_ScreenTex,_ScreenTexScale * i.uv2/i.uv2.w);
                    texcol *= _MainColor  *texScreen;
                    texcol.rgb += i.color;
                    return texcol  *_ColorScale;
                }
            ENDCG 
			}
		}
    }
}
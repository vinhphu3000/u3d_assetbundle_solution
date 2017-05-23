// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "TGame/AlphaTranspreant-flow" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
		_FlowTex ("Flow Texture", 2D) = "black" {}
		_FlowColor("Flow Color", Color) = (1,1,1,1)
    	_NotVisibleColor ("NotVisibleColor (RGB)", Color) = (1,1,1,0.3)
		_XSpeed ("XSpeed", float) = 20
		_YSpeed ("YSpeed", float) = 20
    }
    SubShader {
         Tags { "Queue" = "Geometry+500" "RenderType"="Opaque" }
         LOD 200

     /*    Pass {
            ZTest Greater
            Lighting Off
            ZWrite Off
            //Color [_NotVisibleColor]
            Blend SrcAlpha OneMinusSrcAlpha
            SetTexture [_MainTex] { ConstantColor [_NotVisibleColor] combine constant * texture }

        }
		*/

        Pass {
        	ZTest LEqual
       		Lighting Off
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
					float2 uv1 : TEXCOORD01;
                };

                uniform float4 _MainTex_ST;
				uniform float4 _FlowTex_ST;
				uniform float _XSpeed;
				uniform float _YSpeed;
				uniform sampler2D _MainTex;
				uniform sampler2D _FlowTex;
                uniform fixed4 _Color;
                uniform fixed4 _FlowColor;

                v2f vert (appdata_base v) {
                    v2f o;
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv1 = TRANSFORM_TEX(v.texcoord, _FlowTex);
                    return o;
                }

                fixed4 frag(v2f i) : COLOR {
                    fixed4 texcol = tex2D(_MainTex, i.uv);
                    texcol *= _Color;
					fixed4 flowcol =_FlowColor * tex2D(_FlowTex, i.uv1 +_Time.x * half2(_XSpeed,_YSpeed));
                    return  texcol+flowcol*flowcol.a;
                }
            ENDCG
        }


    }
}

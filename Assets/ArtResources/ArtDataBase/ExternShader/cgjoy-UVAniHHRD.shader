Shader "HZEffect/UVAniHHRD" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        [MaterialToggle] _WuAlpha ("无Alpha贴图（不要用在彩色贴图上）", Float ) = 0
        _MainTex_Color ("MainTex_Color", Color) = (1,1,1,1)
        _Tex_WeiYi ("MainTex Y方向上UV位移", Float ) = 0
        [MaterialToggle] _Tex_WeiYiFangXiang ("MainTex X方向上UV位移", Float ) = 0
        _HunHeTex ("混合贴图（也用于扰动）", 2D) = "white" {}
        _HunHe_Color ("混合贴图_Color", Color) = (1,1,1,1)
        _HunHe_DuiBiDu ("混合强度", Float ) = 1.5
        _XiangHuHunHe_Bi ("混合强度辅助", Float ) = 1
        _HunHe_WeiYiX ("混合贴图UV位移", Float ) = 0
        [MaterialToggle] _AlphaONOff ("全局Alpha", Float ) = 0
        _RaoDong_QiangDu ("扰动强度", Float ) = 0.03
        _RaoDong_WeiYiZ ("扰动UV位移", Float ) = 0
        _XiaoRong ("XiaoRong", Range(1, 0)) = 1
        [MaterialToggle] _XRSwap ("反转（黑白哪个部分先消融）", Float ) = 0.01
        _Mask ("Mask", 2D) = "white" {}
        _Alpha ("Alpha", Range(1, 0)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float4 _MainTex_Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _RaoDong_QiangDu;
            uniform float4 _HunHe_Color;
            uniform sampler2D _HunHeTex; uniform float4 _HunHeTex_ST;
            uniform float _HunHe_DuiBiDu;
            uniform float _HunHe_WeiYiX;
            uniform float _RaoDong_WeiYiZ;
            uniform fixed _AlphaONOff;
            uniform float _XiaoRong;
            uniform float _XiangHuHunHe_Bi;
            uniform float _Tex_WeiYi;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform fixed _Tex_WeiYiFangXiang;
            uniform float _Alpha;
            uniform fixed _WuAlpha;
            uniform fixed _XRSwap;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_4424 = _Time + _TimeEditor;
                float node_6930 = (node_4424.g*_Tex_WeiYi);
                float2 node_6088 = (i.uv0+node_6930*float2(0,0.7));
                float4 node_6525 = tex2D(_HunHeTex,node_6088);
                float node_7034 = (node_6525.r*_XiangHuHunHe_Bi);
                float node_9747 = 0.0;
                float2 node_8403 = (i.uv0+(node_4424.r*_RaoDong_WeiYiZ)*float2(0,11));
                float4 node_7553 = tex2D(_HunHeTex,node_8403);
                float2 node_7018 = (i.uv0+(_RaoDong_QiangDu*node_7553.r));
                float2 node_3296 = (node_7018+(node_4424.r*_HunHe_WeiYiX)*float2(1,0));
                float4 node_476 = tex2D(_HunHeTex,node_3296);
                float2 _Tex_WeiYiFangXiang_var = lerp( (node_7018+node_6930*float2(0,1)), (node_7018+node_6930*float2(1,0)), _Tex_WeiYiFangXiang );
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(_Tex_WeiYiFangXiang_var, _MainTex));
                float3 emissive = (((lerp( float3(node_9747,node_9747,node_9747), (lerp( (_HunHe_Color.rgb*(_HunHe_DuiBiDu*node_6525.rgb)), (_HunHe_DuiBiDu*node_476.rgb*_HunHe_Color.rgb), node_6525.r.r )), node_7034.r ))+(_MainTex_Color.rgb*_MainTex_var.rgb))*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(i.uv0, _Mask));
                float node_4991 = clamp(node_476.r,0.01,1);
                fixed4 finalRGBA = fixed4(finalColor,(i.vertexColor.a*((lerp( lerp( _MainTex_var.a, _MainTex_var.r, _WuAlpha ), 1.0, _AlphaONOff )-(1.0 - _Mask_var.r))-(1.0 - step(lerp( node_4991, (1.0 - node_4991), _XRSwap ),_XiaoRong)))*_MainTex_Color.a*_Alpha));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

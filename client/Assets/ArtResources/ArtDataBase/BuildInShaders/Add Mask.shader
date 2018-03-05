// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Particles/Additive Mask" {
Properties {
 _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
 _MainTex ("Main Texture", 2D) = "white" {}
 _MaskTex ("Mask Texture (RG)", 2D) = "white" {}
}

Category {
 Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
 Blend SrcAlpha One
 Cull Off Lighting Off ZWrite Off
 BindChannels {
     Bind "Color", color
     Bind "Vertex", vertex
     Bind "TexCoord", texcoord
 }

 // ---- Fragment program cards
 SubShader {
     Pass {

         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #pragma fragmentoption ARB_precision_hint_fastest
         #pragma multi_compile_particles

         #include "UnityCG.cginc"

         sampler2D _MainTex;
         sampler2D _MaskTex;
         fixed4 _MainTex_ST;
         fixed4 _MaskTex_ST;
         half _ScrollTimeX;
         half _ScrollTimeY;
         fixed4 _TintColor;

         struct appdata_t {
             float4 vertex : POSITION;
             fixed4 color : COLOR;
             float2 texcoord : TEXCOORD0;
             float2 texcoord2 : TEXCOORD1;
         };

         struct v2f {
             float4 vertex : POSITION;
             fixed4 color : COLOR;
             float2 texcoord : TEXCOORD0;
             float2 texcoord2 : TEXCOORD1;
         };

         v2f vert (appdata_t v)
         {
             v2f o;
             o.vertex = UnityObjectToClipPos(v.vertex);
             o.color = v.color;
             o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
             o.texcoord2 = TRANSFORM_TEX(v.texcoord2, _MaskTex);
             return o;
         }
         fixed4 frag (v2f i) : COLOR
         {
             fixed4 mainColor = tex2D(_MainTex, i.texcoord);
             fixed4 maskColor = tex2D(_MaskTex, i.texcoord2);
             return 2.0f * i.color * _TintColor * mainColor * maskColor;
         }
         ENDCG
     }
 }
}
}

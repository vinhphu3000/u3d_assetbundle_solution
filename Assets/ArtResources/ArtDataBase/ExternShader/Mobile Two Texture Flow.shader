Shader "Mobile/TwoTextureFlow" {
Properties {
	_Color1 ("Color1", Color) = (0.5,0.5,0.5,0.5)
	_Color2 ("Color2", Color) = (0.5,0.5,0.5,0.5)
	_Texture1 ("Texture1", 2D) = "white" {} 
	_Tex1Weight("Tex1Weight", float) = 0.5
	_Texture2 ("Texture2", 2D) = "white" {} 
	_Tex2Weight("Tex2Weight", float) = 0.5
	_Speed ("Speed (tex1 x,y; tex2 x,y)", Vector) = (1,2,3,4)
	
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Blend SrcAlpha One
	 Blend SrcAlpha OneMinusSrcAlpha
	//AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _Texture1;
			sampler2D _Texture2;
			fixed4 _Color1;
			fixed4 _Color2;
			float4 _Speed;
			float _Tex1Weight;
			float _Tex2Weight;
			float _Scale;
			
			struct appdata_t {
				float4 vertex : POSITION; 
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION; 
				float2 texcoord : TEXCOORD0;   
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);  
				o.texcoord = v.texcoord; 
				return o;
			} 
			 
			fixed4 frag (v2f i) : SV_Target
			{  
			float ft = fmod(_Time.x,1);
			   fixed4  c1 =  tex2D(_Texture1, i.texcoord +  ft*_Speed.xy) * _Color1 * _Tex1Weight; 
			   fixed4  c2 =  tex2D(_Texture2, i.texcoord + ft * _Speed.zw) * _Color2 * _Tex2Weight;
			   return c1 + c2;
			}
			ENDCG 
		}
	}	
}
}

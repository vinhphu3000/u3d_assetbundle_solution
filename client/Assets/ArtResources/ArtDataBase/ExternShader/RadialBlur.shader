// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RadialBlur" {
        Properties {
                _MainTex ("Base (RGB)", 2D) = "white" {}
                _fSampleDist("SampleDist", Float) = 0.26
				_X("X",Float) = 0.5
				_Y("Y",Float) = 0.5
                _fSampleStrength("SampleStrength", Float) = 26.3
        }
        SubShader {
                Pass {                 
                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        //#pragma fragmentoption ARB_precision_hint_fastest
        
                        #include "UnityCG.cginc"
        
                        struct appdata_t {
                                float4 vertex : POSITION;
                                half2 texcoord : TEXCOORD0;
                        };
        
                        struct v2f {
                                float4 vertex : SV_POSITION;
                                half2 texcoord : TEXCOORD0;
                        };
                        
                        float4 _MainTex_ST;
                        float _X;
						float _Y;
						
                        v2f vert (appdata_t v)
                        {
                                v2f o;
                                o.vertex = UnityObjectToClipPos(v.vertex);
                                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                                return o;
                        }
        
                        sampler2D _MainTex;
                        float _fSampleDist;
                        float _fSampleStrength;

                        // some sample positions   
                        static const half samples[8] =   
                        {  
						   -0.08, 
                           -0.06,  
                           -0.04,  
						   -0.02, 
                           0.0,  
                           0.01,  
                           0.02,
						   0.03
                        }; 
                        
                        half4 frag (v2f i) : COLOR
                        { 
                           half2 dir = float2(_X, _Y) - i.texcoord;  
                           half2 texcoord = i.texcoord; 
                           half dist = length(dir);   
                           dir /= dist;  
 
                           half4 color = tex2D(_MainTex, texcoord);  

                           half4 sum = color;
                    
                            for (int i = 0; i < 8; ++i)  
                            {  
                              sum += tex2D(_MainTex, texcoord + dir*samples[i]*_fSampleDist);
                           }   
                      
                           sum *=0.125;   
                           
                           float t = saturate(dist * _fSampleStrength);  
 
                           return lerp(color, sum, t);
                        }
                        ENDCG 
                }
        } 
}

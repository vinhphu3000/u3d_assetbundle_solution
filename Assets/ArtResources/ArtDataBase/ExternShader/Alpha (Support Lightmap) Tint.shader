Shader "Alice/Transparent/Alpha (Support Lightmap) Tint" {
Properties {
	_TintColor ("Tint Color", Color) = (1,1,1,1)
	_MainTex ("Texture(RGBA)", 2D) = "white" {}
	//_Cutoff ("Alpha cutoff", Range (0,1)) = 0.1
}

Category {
	LOD 200
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//AlphaTest Greater .1
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Back Lighting Off ZWrite Off
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	
	SubShader 
	{
		Pass 
		{
			Tags { "LightMode" = "Vertex" }
			//AlphaTest Greater [_Cutoff]
			SetTexture [_MainTex] 
			{
				constantColor [_TintColor]
				combine constant * primary
			}
			SetTexture [_MainTex] 
			{
				combine texture * previous
			}
		}


		// Lightmapped, encoded as dLDR
		Pass 
		{
			Tags { "LightMode" = "VertexLM" }

			Lighting Off
			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
		
			SetTexture [unity_Lightmap] 
			{
				matrix [unity_LightmapMatrix]
				combine texture
			}

			SetTexture [_MainTex] 
			{
				combine texture * previous DOUBLE, texture * primary
			}
			
			SetTexture [_MainTex] 
			{
				constantColor [_TintColor]
				combine constant * previous, previous
			}
		}
	
		// Lightmapped, encoded as RGBM
		Pass 
		{
			Tags { "LightMode" = "VertexLMRGBM" }
		
			Lighting Off
			BindChannels {
				Bind "Vertex", vertex
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
		
			SetTexture [unity_Lightmap] 
			{
				matrix [unity_LightmapMatrix]
				combine texture * texture alpha DOUBLE
			}
			SetTexture [_MainTex] 
			{
				combine texture * previous QUAD, texture * primary
			}
			SetTexture [_MainTex] 
			{
				constantColor [_TintColor]
				combine constant * previous, previous
			}
		}	
	}
}
Fallback "Alice/Transparent/Alpha"
}

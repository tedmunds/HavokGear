Shader "Sprites/Default Tiled" {

    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Scale ("Scaling", Float) = 1.0
    }


    SubShader {
        Tags {
			"Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
		ZTest Always
		Cull Off
		ZWrite Off
		Lighting Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {      
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
				#pragma multi_compile DUMMY PIXELSNAP_ON
               
                #include "UnityCG.cginc"
       
                struct appdata {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD;
                    float4 color: COLOR;
                };
               
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD;
                    float4 color: COLOR;
                };
               
                uniform sampler2D _MainTex;
				uniform float _Scale;

                v2f vert (appdata v)  
                {
                    v2f o;
                    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                    o.uv = v.texcoord;
					o.color = v.color;
					#ifdef PIXELSNAP_ON
					o.pos = UnityPixelSnap (o.pos);
					#endif

                    return o;
                }
               
                half4 frag(v2f i) : SV_Target
                {          
                    //float xScale = length(float3(_Object2World[0][0], _Object2World[1][0], _Object2World[2][0]));
                    //float yScale = length(float3(_Object2World[0][1], _Object2World[1][1], _Object2World[2][1]));
					
					float2 tiledUv = mul(_Object2World, float4(i.uv, 0, 0)).xy / _Scale;
					tiledUv = fmod(tiledUv, float2(1, 1));
					half4 c = tex2D(_MainTex, tiledUv) * i.color;

                    //half4 c = tex2D(_MainTex, fmod(i.uv * float2(xScale ,yScale),1)) * i.color;

					//half4 c = tex2D(_MainTex, fmod(i.uv*float2(_ScaleX,_ScaleY),1)) * i.color;

                    return c;
                }
   
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
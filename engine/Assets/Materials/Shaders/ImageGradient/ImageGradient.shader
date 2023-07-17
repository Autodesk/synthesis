Shader "ImageGradient/ImageGradient" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off

        Pass {
            CGPROGRAM

            #include "SDFUtils.cginc"
            #include "ShaderSetup.cginc"
            
            #pragma vertex vert
            #pragma fragment frag

            float4 _WidthHeightRadius;
            float4 _LeftColor;
            float4 _RightColor;

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;

            fixed4 frag (v2f i) : SV_Target {
                float alpha = CalcAlpha(i.uv, _WidthHeightRadius.xy, _WidthHeightRadius.z);
                
                float4 col = lerp(_LeftColor, _RightColor, i.uv.x);
                return float4(col.x, col.y, col.z, alpha);
            }
            
            ENDCG
        }
    }
}

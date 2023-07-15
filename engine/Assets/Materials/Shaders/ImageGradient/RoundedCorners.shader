Shader "UI/RoundedCorners/RoundedCorners" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        /*[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0*/
        
    }
    
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        
        /*Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }*/
        /*Cull Off
        Lighting Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]*/

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off

        Pass {
            CGPROGRAM
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"          
            #include "SDFUtils.cginc"
            #include "ShaderSetup.cginc"
            
            #pragma vertex vert
            #pragma fragment frag

            /*#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP*/

            float4 _WidthHeightRadius;
            float4 _LeftColor;
            float4 _RightColor;

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;

            fixed4 frag (v2f i) : SV_Target {
                //half4 color = (tex2D(_MainTex, i.uv)) * i.color;

                /*if (color.a <= 0) {
                    return color;
                }*/
                //return color;
                
                float alpha = CalcAlpha(i.uv, _WidthHeightRadius.xy, _WidthHeightRadius.z);
                
                float4 col = lerp(_LeftColor, _RightColor, i.uv.x);
                return float4(col.x, col.y, col.z, alpha);
                
            }
            
            ENDCG
        }
    }
}

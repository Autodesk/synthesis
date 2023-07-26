Shader "ImageGradient/ImageGradient" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        
        _ColorMask ("Color Mask", Float) = 15
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }
    
    SubShader {
        Tags {
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        //Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {
            CGPROGRAM

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "SDFUtils.cginc"
            #include "ShaderSetup.cginc"
            
            #pragma vertex vert
            #pragma fragment frag

            float4 _WidthHeightRadius;
            float4 _StartColor;
            float4 _EndColor;
            float4 _TintColor;
            float4 _ClipRect;
            float _Offset;
            int _Horizontal;

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;

            fixed4 frag (v2f i) : SV_Target {
                
                float maskAlpha = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                float alpha = CalcAlpha(i.uv, _WidthHeightRadius.xy, _WidthHeightRadius.z);

                float gradientPos = (_Horizontal > 0 ? i.uv.x : 1 - i.uv.y) + _Offset;
                if (gradientPos > 1)
                    gradientPos = 2 - gradientPos;
                
                float4 col = lerp(_StartColor, _EndColor, _Horizontal > 0 ? i.uv.x : 1 - i.uv.y);
                return float4(col.x, col.y, col.z, alpha * maskAlpha * col.a)*_TintColor;
            }
            
            ENDCG
        }
    }
}

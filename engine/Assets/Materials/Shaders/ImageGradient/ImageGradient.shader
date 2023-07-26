Shader "ImageGradient/ImageGradient" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        _GradientAngle ("Gradient Angle", Float) = 0
        _GradientSpread ("Gradient Spread", Range(0.1, 2)) = 1

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

            float4 _ClipRect;

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;

            float _GradientAngle;
            float _GradientSpread;

            fixed4 frag (v2f i) : SV_Target {
                float maskAlpha = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                float alpha = CalcAlpha(i.uv, _WidthHeightRadius.xy, _WidthHeightRadius.z);

                // _GradientAngle = 3.14159 * (3.0 / 2.0);

                float4 gradientDirection = float4(cos(_GradientAngle), sin(_GradientAngle), 0, 1);
                gradientDirection *= _GradientSpread;

                float2 uvOffset = (gradientDirection.xy / 2) - float2(0.5, 0.5);

                float4 col = lerp(_StartColor, _EndColor, clamp(dot(gradientDirection.xy, i.uv + uvOffset), 0, 1));
                // float4 col = lerp(_StartColor, _EndColor, _Horizontal > 0 ? i.uv.x : 1 - i.uv.y);

                //return float4(col.x, col.y, col.z, alpha*maskAlpha);
                return float4(col.x, col.y, col.z, alpha * maskAlpha * col.a);
            }
            
            ENDCG
        }
    }
}

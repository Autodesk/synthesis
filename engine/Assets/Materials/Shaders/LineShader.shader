Shader "Line/LineFullRounded"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Stroke ("Stroke", Int) = 20
        _PointA ("Point A", Vector) = (500, 500, 0)
        _PointB ("Point B", Vector) = (600, 1000, 0)
        _ActualScreenParams ("Actual Screen Params", Vector) = (1920, 1080, 0)
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB

        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float4 _Color;
            uniform float _Stroke;
            uniform float4 _PointA;
            uniform float4 _PointB;
            uniform float4 _ActualScreenParams;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float2 getScreenPos(float4 vertex) {
                float conversionFactor = 1080.0 / _ActualScreenParams.y; // Scale matching height (Same as canvas)
                return vertex.xy * conversionFactor;
            }

            float2 relativeDistances(float2 p, float2 targetA, float2 targetB) {
                float2 t_vec = targetB - targetA;
                float2 p_vec = p - targetA;

                float x = length(t_vec) * dot(t_vec / length(t_vec), p_vec / length(t_vec));
                float y = sqrt(abs(pow(length(p_vec), 2) - pow(x, 2)));

                return float2(x, y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = float4(0,0,0,0);

                fixed2 a = _PointA.xy;
                fixed2 b = _PointB.xy;
                float2 c = (getScreenPos(i.vertex));

                float2 distA = relativeDistances(c, a, b);
                float2 distB = relativeDistances(c, b, a);

                if (distA.x < 0 || distB.x < 0) {
                    if (distance(c, a) < _Stroke / 2.0 || distance(c, b) < _Stroke / 2.0) {
                        color = float4(1,1,1,1);
                    }
                } else {
                    if (distA.y <= _Stroke / 2.0) {
                        color = float4(1,1,1,1);
                    }
                }

                color *= _Color;

                // if (distance(a, c) < 10) {
                //     color = float4(0, 1, 0, 1);
                // }
                // if (distance(b, c) < 10) {
                //     color = float4(1, 0, 1, 1);
                // }

                // color.a = 0.0;
                return color;
            }
            ENDCG
        }
    }
}

Shader "Custom/TransparentShadow"
{
    Properties
    {
        [Header(Shadow)]
        [Space(5)]
        _ShadowWeight("Shadow Weight", Range(0,1)) = 0.5

        [Header(Grid)]
        [Space(5)]
        _GridFocusPoint("Focus Point", COlor) = (0, 0, 0, 1)
        _GridColor("Color", Color) = (1, 1, 1, 1)
        _GridUnit("Unit Size", Range(1, 10)) = 1.0
        _GridFalloffRadius("Falloff Radius", Range(0.1, 100)) = 20
        _GridFalloffFeatherDistance("Falloff Feather Distance", Range(0.0001, 100)) = 10
        
        [Header(LineSpecifics)]
        [Space(5)]
        _GridLineWidth("Grid Line Width", Range(0,0.3)) = 0.04
        _GridLineFeather("Grid Line Feather", Range(0.0, 0.5)) = 0.0
        _GridBoldInterval("Grid Bold Interval", Range(3, 10)) = 5
        _GridLineWidthBold("Grid Line Width Bold", Range(0.005, 0.2)) = 0.08
        _GridLineFeatherBold("Grid Line Feather Bold", Range(0.0, 0.5)) = 0.0
    }  

    SubShader
    {
        
        Name  "URPDefault"

        Tags {"RenderPipeline"="UniversalRenderPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
        
        LOD 300
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {          
            HLSLPROGRAM

            // Conflicts
            // #include "UnityCG.cginc"

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            
            //include fog
            #pragma multi_compile_fog           

            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            // #pragma shader_feature _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)

            float   _ShadowWeight;

            float4  _GridFocusPoint;

            float4  _GridColor;
            float   _GridLineWidth;
            float   _GridUnit;
            int     _GridBoldInterval;
            float   _GridLineWidthBold;
            float   _GridLineFeather;
            float   _GridLineFeatherBold;

            float   _GridFalloffRadius;
            float   _GridFalloffFeatherDistance;

            uniform float4x4 _ProjectMat;

            CBUFFER_END


            
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID                              
            };

            struct VertexOutput
            {
                float4 vertex               : SV_POSITION;
                float2 uv                   : TEXCOORD0;
                float fogCoord              : TEXCOORD1;
                float3 normal               : NORMAL;
                float4 worldVertex          : fixed4;
                float4x4 cameraProjection   : float4x4;
                
                float4 shadowCoord : TEXCOORD2;
                
                //if shader need a view direction, use below code in shader 
                //float3 WorldSpaceViewDirection : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                o.worldVertex = float4(TransformObjectToWorld(v.vertex.xyz), 1);

                o.cameraProjection = GetWorldToViewMatrix();
                
                o.uv = v.uv.xy;
                // o.normal = normalize(mul(v.normal, (float3x3)UNITY_MATRIX_I_M));
                o.normal = float3(0,1,0);

                // o.fogCoord = ComputeFogFactor(o.vertex.z);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.shadowCoord = GetShadowCoord(vertexInput);
                
                //view direction
                //o.WorldSpaceViewDirection = _WorldSpaceCameraPos.xyz - mul(GetObjectToWorldMatrix(), float4(v.vertex.xyz, 1.0)).xyz;
                

                return o;
            }

            float gridUnitMod(float axisPos, float unit)
            {
                return fmod(axisPos, unit)
                    + unit * step(0, -axisPos);
            }

            float shouldGrid(float axisPos, float thickness, float feather)
            {
                float mod = gridUnitMod(axisPos, _GridUnit);
                float roundMod = 1;
                if (mod < thickness * feather)
                {
                    return (mod / (thickness * feather));
                }
                else if (mod > thickness * (1 - feather) && mod <= thickness)
                {
                    return max(
                        0,
                        1 - ((mod - (thickness * (1 - feather))) / (thickness * feather))
                    );
                } else {
                    return 1 - step(thickness, mod);
                } 
            }

            float4 transformPoint(float4 worldPos)
            {
                float4 res = TransformObjectToHClip(worldPos.xyz);
                res.x *= _ScreenParams.x;
                res.y *= _ScreenParams.y;
                return res;
            }

            float4 projectionMat(float4x4 proj, float4 coord) {
                return mul(proj, coord.xzyw);
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // return float4(UNITY_MATRIX_I_V._14_24_34, 1);

                // return float4(i.vertex.xy * 0.0005, 0, 1);




                // float3 lookDir = GetViewForwardDir();
                // lookDir -= float3(0, dot(float3(0, 1, 0), lookDir), 0);
                // lookDir = normalize(lookDir);

                // return float4(lookDir, 1);

                // float projectedSize = distance(
                //     transformPoint(float4(i.worldVertex + (lookDir * _GridLineWidth), 1)),
                //     transformPoint(float4(i.worldVertex + float3(0,0,0), 1))
                // );

                // return transformPoint(i.worldVertex, i.cameraProjection);

                // return float4(projectedSize, projectedSize, projectedSize, 1);

                float isBoldX = (1 - step(
                    0.999,
                    floor(gridUnitMod(i.worldVertex.x, _GridUnit * _GridBoldInterval))
                ));
                float isBoldZ = (1 - step(
                    0.999,
                    floor(gridUnitMod(i.worldVertex.z, _GridUnit * _GridBoldInterval))
                ));

                float shouldX = shouldGrid(i.worldVertex.x, lerp(
                        _GridLineWidth,
                        _GridLineWidthBold,
                        isBoldX
                    ),
                    lerp(_GridLineFeather, _GridLineFeatherBold, isBoldX)
                );
                float shouldZ = shouldGrid(i.worldVertex.z, lerp(
                        _GridLineWidth,
                        _GridLineWidthBold,
                        isBoldZ
                    ),
                    lerp(_GridLineFeather, _GridLineFeatherBold, isBoldZ)
                );
                float shouldGrid = max(shouldZ, shouldX);

                float distFromFocus = distance(i.worldVertex.xyz, _GridFocusPoint.xyz);
                shouldGrid = lerp(
                    shouldGrid,
                    lerp(shouldGrid, 0, (distFromFocus - _GridFalloffRadius) / _GridFalloffFeatherDistance),
                    step(0, distFromFocus - _GridFalloffRadius)
                );
                
                // float4 col = tex2D(_MainTex, i.uv) * _TintColor;
                float4 col = float4(_GridColor.xyz, _GridColor.w * shouldGrid);
                // float4 col = float4(0,0,0,1);

                //below texture sampling code does not use in material inspector             
                // float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                Light mainLight = GetMainLight(i.shadowCoord);

                //Lighting Calculate(Lambert)              
                float NdotL = saturate(dot(normalize(_MainLightPosition.xyz), i.normal));            
                float3 ambient = SampleSH(i.normal);             

                // half receiveshadow = MainLightRealtimeShadow(i.shadowCoord);
                // col.rgb *= NdotL * _MainLightColor.rgb * receiveshadow + ambient;

                col.rgb *= NdotL * _MainLightColor.rgb * mainLight.shadowAttenuation + ambient;
                
                col.a = max((1 - mainLight.shadowAttenuation) * _ShadowWeight, col.a);

                return col;            
            }

            ENDHLSL  
        }

        Pass
        {
            Name "ShadowCaster"

            Tags{"LightMode" = "ShadowCaster"}

            Cull Back

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #pragma shader_feature _ALPHATEST_ON

            // GPU Instancing
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"


            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float   _Alpha;
            CBUFFER_END

            struct VertexInput
            {         
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                
                #if _ALPHATEST_ON
                    float2 uv     : TEXCOORD0;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };
            
            struct VertexOutput
            {         
                float4 vertex : SV_POSITION;
                #if _ALPHATEST_ON
                    float2 uv     : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID         
                UNITY_VERTEX_OUTPUT_STEREO
                
            };

            VertexOutput ShadowPassVertex(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);                            
                
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(v.normal.xyz);
                
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                
                o.vertex = positionCS;
                #if _ALPHATEST_ON
                    o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; ;
                #endif

                return o;
            }

            half4 ShadowPassFragment(VertexOutput i) : SV_TARGET
            {  
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                #if _ALPHATEST_ON
                    float4 col = tex2D(_MainTex, i.uv);
                    clip(col.a - _Alpha);
                #endif

                return 0;
            }

            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            Cull Back

            HLSLPROGRAM
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
            
            struct VertexInput
            {
                float4 vertex : POSITION;                   
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {           
                float4 vertex : SV_POSITION;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID           
                UNITY_VERTEX_OUTPUT_STEREO                 
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                return o;
            }

            half4 frag(VertexOutput IN) : SV_TARGET
            {       
                return 0;
            }
            ENDHLSL
        }
        
    }
}

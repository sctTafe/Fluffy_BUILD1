Shader "Custom/GhostFresnelEdge_URP"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (1,0,0,1)
        _EdgePower ("Edge Power", Range(1,10)) = 3
        _StencilRef ("Stencil Reference", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        //-------------------------------------------------------------------------
        // Pass 1: Stencil Mask Pass
        // This pass marks pixels that belong to this material in the stencil buffer
        // and writes depth to prevent back faces from showing through
        //-------------------------------------------------------------------------
        Pass
        {
            Name "StencilMask"
            Cull Front                       // Render back faces first
            ZWrite On                        // Write depth
            ZTest LEqual                     // Standard depth test
            ColorMask 0                      // Don't write color
            
            Stencil
            {
                Ref [_StencilRef]
                Comp Always                  // Always pass stencil test
                Pass Replace                 // Replace stencil value
            }

            HLSLPROGRAM
            #pragma vertex VertDepth
            #pragma fragment FragDepth
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesDepth
            {
                float4 positionOS : POSITION;
            };

            struct VaryingsDepth
            {
                float4 positionHCS : SV_POSITION;
            };

            VaryingsDepth VertDepth(AttributesDepth IN)
            {
                VaryingsDepth OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                return OUT;
            }

            half4 FragDepth(VaryingsDepth IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        //-------------------------------------------------------------------------
        // Pass 2: Front Face Depth Pass
        // This pass renders front faces and writes to depth buffer
        //-------------------------------------------------------------------------
        Pass
        {
            Name "FrontDepth"
            Cull Back                        // Render front faces
            ZWrite On                        // Write depth
            ZTest LEqual                     // Standard depth test
            ColorMask 0                      // Don't write color
            
            Stencil
            {
                Ref [_StencilRef]
                Comp Always                  // Always pass stencil test
                Pass Replace                 // Replace stencil value
            }

            HLSLPROGRAM
            #pragma vertex VertDepth
            #pragma fragment FragDepth
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesDepth
            {
                float4 positionOS : POSITION;
            };

            struct VaryingsDepth
            {
                float4 positionHCS : SV_POSITION;
            };

            VaryingsDepth VertDepth(AttributesDepth IN)
            {
                VaryingsDepth OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                return OUT;
            }

            half4 FragDepth(VaryingsDepth IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        //-------------------------------------------------------------------------
        // Pass 3: Ghost Render with Fresnel Edge Effect
        // This pass only renders where stencil buffer doesn't match our reference
        // This prevents overlapping faces from the same material from showing
        //-------------------------------------------------------------------------
        Pass
        {
            Name "GhostFresnelPass"
            Tags { "LightMode"="UniversalForward" }
            Cull Back                        // Render front faces only
            ZWrite Off                       // Don't write depth in final pass
            Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending
            ZTest LEqual                     // Use existing depth
            
            Stencil
            {
                Ref [_StencilRef]
                Comp NotEqual                // Only render where stencil doesn't match
                ReadMask 255
            }

            HLSLPROGRAM
            #pragma vertex VertMain
            #pragma fragment FragMain
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesMain
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct VaryingsMain
            {
                float4 positionHCS : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
            };

            VaryingsMain VertMain(AttributesMain IN)
            {
                VaryingsMain OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldNormal = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
                return OUT;
            }

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _EdgeColor;
                float  _EdgePower;
                int    _StencilRef;
            CBUFFER_END

            half4 FragMain(VaryingsMain IN) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float ndotv = saturate(dot(IN.worldNormal, viewDir));
                float fresnel = pow(1.0 - ndotv, _EdgePower);
                
                float4 finalColor = lerp(_MainColor, _EdgeColor, fresnel);
                return half4(finalColor.rgb, finalColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
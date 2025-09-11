Shader "Universal Render Pipeline/Particles/OpaqueWithHeightFog"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // Height-based fog properties
        _MinFogHeight("Min Fog Height", Float) = 0.0
        _MaxFogHeight("Max Fog Height", Float) = 10.0
        _MinFogStrength("Min Fog Strength (Top)", Range(0,1)) = 0.0
        _MaxFogStrength("Max Fog Strength (Bottom)", Range(0,1)) = 1.0
        
        // Camera distance-based fog height adjustment
        _CameraDistanceInfluence("Camera Distance Influence", Range(0,1)) = 0.5
        _NearCameraHeightScale("Near Camera Height Scale", Range(0.1,2)) = 0.5
        _FarCameraHeightScale("Far Camera Height Scale", Range(0.1,2)) = 1.5
        _MaxCameraDistance("Max Camera Distance", Float) = 50.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Unlit"
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Cutoff;
                float _MinFogHeight;
                float _MaxFogHeight;
                float _MinFogStrength;
                float _MaxFogStrength;
                float _CameraDistanceInfluence;
                float _NearCameraHeightScale;
                float _FarCameraHeightScale;
                float _MaxCameraDistance;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Particle system color/lifetime color
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 particleColor : COLOR; // Pass particle color to fragment
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                OUT.particleColor = IN.color; // Pass particle system color
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // Sample texture and apply base color and particle system color
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor * IN.particleColor;

                // Alpha cutoff test
                clip(col.a - _Cutoff);

                // Calculate camera distance for fog height adjustment
                float3 cameraPos = GetCameraPositionWS();
                float cameraDistance = distance(cameraPos, IN.worldPos);
                float normalizedCameraDistance = saturate(cameraDistance / _MaxCameraDistance);
                
                // Interpolate height scale based on camera distance
                float heightScale = lerp(_NearCameraHeightScale, _FarCameraHeightScale, normalizedCameraDistance);
                
                // Apply camera distance influence to fog height values
                float adjustedMinFogHeight = lerp(_MinFogHeight, _MinFogHeight * heightScale, _CameraDistanceInfluence);
                float adjustedMaxFogHeight = lerp(_MaxFogHeight, _MaxFogHeight * heightScale, _CameraDistanceInfluence);

                // Calculate height-based fog strength with camera-adjusted heights
                float heightFactor = saturate((IN.worldPos.y - adjustedMinFogHeight) / (adjustedMaxFogHeight - adjustedMinFogHeight));
                float heightBasedFogStrength = lerp(_MaxFogStrength, _MinFogStrength, heightFactor);

                // Get fog color and factor
                half3 fogColor = unity_FogColor.rgb;
                half fogFactor = saturate(IN.fogCoord);

                // Apply height-based fog blend
                half3 fogged = lerp(col.rgb, lerp(col.rgb, fogColor, fogFactor), heightBasedFogStrength);
                col.rgb = fogged;

                return col;
            }
            ENDHLSL
        }
    }
}

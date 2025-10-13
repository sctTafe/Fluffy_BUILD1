Shader "Custom/ParticleDitherSimple"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _DitherAlpha ("Dither Alpha", Range(0,1)) = 1
        _DitherScale ("Dither Pixel Scale", Range(1,8)) = 2
        [Toggle(_AMBIENT_OFF)] _AmbientOff ("Disable Ambient (SH)", Float) = 0
        _ShadowMin ("Shadow Min (Lift)", Range(0,1)) = 0.0
        _AmbientMul ("Ambient Multiplier", Range(0,2)) = 1.0
    }
    SubShader
    {
        // AlphaTest queue keeps ordering predictable; depth writes ON reduce overdraw.
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 25
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "Forward"
            Tags{ "LightMode"="UniversalForward" }
            // Opaque style (no blending) for cheapest path; dither uses clip()
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _AMBIENT_OFF
            #define _ALPHATEST_ON 1

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3  normalWS   : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half  _DitherAlpha;
            half  _DitherScale; // 1..8
            half  _ShadowMin;     // Lifts darkest side of light from 0..1
            half  _AmbientMul;    // Scales ambient SH lighting
            CBUFFER_END

            // 4x4 Bayer matrix normalized 0..1 (half precision OK)
            inline half Dither4x4Bayer(int x, int y)
            {
                const half dither[16] = {
                    half(1.0/17.0), half(9.0/17.0), half(3.0/17.0), half(11.0/17.0),
                    half(13.0/17.0), half(5.0/17.0), half(15.0/17.0), half(7.0/17.0),
                    half(4.0/17.0), half(12.0/17.0), half(2.0/17.0), half(10.0/17.0),
                    half(16.0/17.0), half(8.0/17.0), half(14.0/17.0), half(6.0/17.0) };
                return dither[(y & 3) * 4 + (x & 3)];
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.color      = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Combined fade factor (material * vertex alpha)
                half fade = saturate(_DitherAlpha * IN.color.a);
                int2 pix  = int2(IN.positionCS.xy); // SV_POSITION (post-transform) -> stable per-pixel
                int  scale = max(1, (int)_DitherScale);
                int2 dc = int2((pix.x / scale) & 3, (pix.y / scale) & 3);
                half threshold = Dither4x4Bayer(dc.x, dc.y);
                clip(fade - threshold);

                // Lighting (main directional + optional ambient SH)
                half3 n = normalize(IN.normalWS);
                // Access pre-populated main light params directly (cheaper than GetMainLight())
                // _MainLightPosition.xyz is direction (pointing TO light) in URP when w == 0
                half3 L = normalize(_MainLightPosition.xyz);
                half ndotl = max(0, dot(n, L));
                // Lift shadows: remap ndotl 0..1 to _ShadowMin..1
                ndotl = mad(ndotl, (half)1.0 - _ShadowMin, _ShadowMin); // ndotl*(1-_ShadowMin)+_ShadowMin
                half3 lightColor = _MainLightColor.rgb * ndotl;
                #ifndef _AMBIENT_OFF
                    half3 ambient = SampleSH(n) * _AmbientMul;
                    lightColor += ambient;
                #endif

                half3 rgb = _BaseColor.rgb * IN.color.rgb * lightColor;
                return half4(rgb, 1); // Hard cutout already via clip
            }
            ENDHLSL
        }
    }
    FallBack Off
}
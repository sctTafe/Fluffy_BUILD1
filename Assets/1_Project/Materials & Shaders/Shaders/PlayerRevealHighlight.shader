Shader "Custom/PlayerRevealHighlight"
{
    Properties
    {
        _HighlightColor ("Highlight Color", Color) = (1, 0.2, 0.2, 0.8)
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3
        _IntensityMin ("Min Intensity", Range(0, 1)) = 0.4
        _IntensityMax ("Max Intensity", Range(0, 1)) = 1.0
        _EffectIntensity ("Effect Intensity", Range(0, 1)) = 1.0
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2
        _RimWidth ("Rim Width", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Overlay+100" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        
        Pass
        {
            Name "RevealHighlight"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            fixed4 _HighlightColor;
            float _PulseSpeed;
            float _IntensityMin;
            float _IntensityMax;
            float _EffectIntensity;
            float _FresnelPower;
            float _RimWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel effect for edge highlighting
                float fresnel = 1.0 - saturate(dot(normalize(i.viewDir), normalize(i.worldNormal)));
                fresnel = pow(fresnel, _FresnelPower);
                
                // Enhanced rim effect
                float rim = smoothstep(1.0 - _RimWidth, 1.0, fresnel);
                
                // Pulsing animation with more dynamic range
                float time = _Time.y * _PulseSpeed;
                float pulse1 = sin(time) * 0.5 + 0.5;
                float pulse2 = sin(time * 1.3 + 1.0) * 0.3 + 0.7; // Secondary pulse for complexity
                float combinedPulse = pulse1 * pulse2;
                
                float intensity = lerp(_IntensityMin, _IntensityMax, combinedPulse);
                
                // Combine effects
                float highlight = rim * intensity * _EffectIntensity;
                
                // Distance-based intensity falloff for more realistic effect
                float dist = distance(i.worldPos, _WorldSpaceCameraPos);
                float distanceFade = saturate(1.0 - (dist * 0.01)); // Adjust multiplier as needed
                highlight *= distanceFade;
                
                // Color with highlight
                fixed4 col = _HighlightColor;
                col.a *= highlight;
                
                // Add some color variation based on angle
                float3 viewDirWS = normalize(i.viewDir);
                float angleVariation = dot(viewDirWS, float3(0, 1, 0)) * 0.1;
                col.rgb += angleVariation;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback Off
}
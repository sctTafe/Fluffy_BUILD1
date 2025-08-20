Shader "Custom/PredatorVisionHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HighlightColor ("Highlight Color", Color) = (1, 0, 0, 1)
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 0.3
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2
        _RimWidth ("Rim Width", Range(0, 1)) = 0.5
        _Transparency ("Transparency", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent+200" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        
        Pass
        {
            Name "PredatorVision"
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _HighlightColor;
            float _PulseSpeed;
            float _PulseIntensity;
            float _FresnelPower;
            float _RimWidth;
            float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Base texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate fresnel effect for rim lighting
                float fresnel = 1.0 - dot(normalize(i.viewDir), normalize(i.worldNormal));
                fresnel = pow(fresnel, _FresnelPower);
                
                // Pulsing effect
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseIntensity + (1.0 - _PulseIntensity);
                
                // Combine effects
                float rimEffect = smoothstep(1.0 - _RimWidth, 1.0, fresnel);
                float highlightStrength = rimEffect * pulse;
                
                // Final color with highlight
                col.rgb = lerp(col.rgb, _HighlightColor.rgb, highlightStrength);
                col.a = _Transparency * _HighlightColor.a * highlightStrength;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/Diffuse"
}
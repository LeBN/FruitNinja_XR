// Fantôme du fruit de difficulté choisi : translucide, liseré lumineux (fresnel) et légère pulsation.
// Compatible Single Pass Instanced (macros stéréo).
Shader "FruitNinja/Phantom"
{
    Properties
    {
        _Color ("Couleur de base", Color) = (0.55, 0.8, 1, 0.18)
        _RimColor ("Couleur du liseré", Color) = (0.75, 0.92, 1, 0.9)
        _RimPower ("Finesse du liseré", Float) = 2.5
        _PulseSpeed ("Vitesse de pulsation", Float) = 2.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Phantom"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _RimColor;
                float _RimPower;
                float _PulseSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 n = normalize(i.normalWS);
                float3 viewDir = normalize(GetWorldSpaceViewDir(i.positionWS));
                float rim = pow(1.0 - saturate(dot(n, viewDir)), _RimPower);
                float pulse = 0.85 + 0.15 * sin(_Time.y * _PulseSpeed);
                half3 col = _Color.rgb + _RimColor.rgb * rim;
                half alpha = saturate(_Color.a + _RimColor.a * rim) * pulse;
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}

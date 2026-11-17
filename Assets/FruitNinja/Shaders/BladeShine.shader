// Reflet lumineux qui parcourt la lame : bande blanche (dans la largeur) qui glisse
// de la garde à la pointe. Passe additive ajoutée en second matériau sur le sabre.
// Compatible Single Pass Instanced (macros stéréo).
Shader "FruitNinja/BladeShine"
{
    Properties
    {
        _Color ("Couleur", Color) = (1, 1, 1, 1)
        _Intensity ("Intensité", Float) = 3
        _BladeAxis ("Axe de la lame (espace objet)", Vector) = (0, 1, 0, 0)
        _BladeStart ("Début de lame (garde)", Float) = 0
        _BladeEnd ("Fin de lame (pointe)", Float) = 1
        _BandWidth ("Largeur de la bande", Float) = 0.05
        _Progress ("Progression (0-1, <0 = caché)", Float) = -1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "BladeShine"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Intensity;
                float4 _BladeAxis;
                float _BladeStart;
                float _BladeEnd;
                float _BandWidth;
                float _Progress;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float axisPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.axisPos = dot(v.positionOS.xyz, normalize(_BladeAxis.xyz));
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                if (_Progress < 0.0 || _Progress > 1.0)
                    return 0;

                float lo = min(_BladeStart, _BladeEnd);
                float hi = max(_BladeStart, _BladeEnd);
                float inside = step(lo, i.axisPos) * step(i.axisPos, hi);

                float center = lerp(_BladeStart, _BladeEnd, _Progress);
                float d = (i.axisPos - center) / max(_BandWidth, 1e-4);
                float band = exp(-d * d);

                // Apparition / disparition douces aux extrémités du balayage
                float fade = smoothstep(0.0, 0.08, _Progress) * smoothstep(1.0, 0.85, _Progress);

                return half4(_Color.rgb * (band * inside * fade * _Intensity), 0);
            }
            ENDHLSL
        }
    }
}

// Shader de particules léger (PCVR et Quest) : lit un canal de la texture comme masque,
// couleur = couleur de la particule × teinte. Mode additif ou transparent (prémultiplié).
// Remplace les shaders "BIRP" des packs Vefects, non affichables en URP.
// Compatible Single Pass Instanced (macros stéréo).
Shader "FruitNinja/ParticleMask"
{
    Properties
    {
        _MainTex ("Texture (masque)", 2D) = "white" {}
        _Channel ("Canal du masque (RGBA)", Vector) = (1, 0, 0, 0)
        [HDR] _Tint ("Teinte", Color) = (1, 1, 1, 1)
        [Toggle] _Additive ("Additif", Float) = 1
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

        Pass
        {
            Name "ParticleMask"
            Tags { "LightMode" = "UniversalForward" }

            Blend One [_DstBlend]
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Channel;
                half4 _Tint;
                float _Additive;
                float _DstBlend;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half mask = saturate(dot(tex, _Channel));
                half4 col = i.color * _Tint;
                half a = saturate(col.a * mask);
                // Prémultiplié : additif -> alpha 0 (Blend One One), transparent -> Blend One OneMinusSrcAlpha
                return half4(col.rgb * a, _Additive > 0.5 ? 0 : a);
            }
            ENDHLSL
        }
    }
}

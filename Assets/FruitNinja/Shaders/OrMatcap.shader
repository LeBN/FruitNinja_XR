// Shader « reflet doré précalculé » (matcap) pour la pomme d'or : la couleur vient d'une image de sphère
// dorée éclairée, lue selon l'orientation de la surface vue par la caméra. Toujours brillante, même la nuit,
// et très légère (une texture, pas de lumière ni de reflet temps réel). Compatible Single Pass Instanced.
Shader "FruitNinja/OrMatcap"
{
    Properties
    {
        _Matcap ("Matcap (reflet précalculé)", 2D) = "white" {}
        _Couleur ("Teinte", Color) = (1, 1, 1, 1)
        _Eclat ("Éclat", Float) = 1.15
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Matcap"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Matcap);
            SAMPLER(sampler_Matcap);

            CBUFFER_START(UnityPerMaterial)
                half4 _Couleur;
                half _Eclat;
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
                float2 uvMatcap : TEXCOORD0;
                half fog : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));
                o.uvMatcap = normalVS.xy * 0.5 + 0.5;
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 c = SAMPLE_TEXTURE2D(_Matcap, sampler_Matcap, i.uvMatcap).rgb * _Couleur.rgb * _Eclat;
                c = MixFog(c, i.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}

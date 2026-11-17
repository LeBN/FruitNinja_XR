// Or entièrement métallique pour la pomme d'or : la couleur vient du reflet de l'environnement,
// précalculé une fois dans une cubemap (capture de la scène depuis la zone de jeu), teinté en or.
// + reflet de la lune (lumière principale) et bords plus brillants (Fresnel).
// Léger (une cubemap, pas de sonde temps réel), compatible Single Pass Instanced et brouillard.
Shader "FruitNinja/OrReflet"
{
    Properties
    {
        [NoScaleOffset] _Environnement ("Reflet précalculé (cubemap)", Cube) = "" {}
        _Teinte ("Teinte de l'or", Color) = (1, 0.78, 0.32, 1)
        _Intensite ("Intensité du reflet", Float) = 2.5
        _Ambiant ("Lumière minimale", Float) = 0.18
        _Fresnel ("Bords brillants", Float) = 0.6
        _Eclat ("Reflet de la lune", Float) = 1.5
        _Brillance ("Netteté du reflet de la lune", Float) = 64
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "OrReflet"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURECUBE(_Environnement);
            SAMPLER(sampler_Environnement);

            CBUFFER_START(UnityPerMaterial)
                half4 _Teinte;
                half _Intensite;
                half _Ambiant;
                half _Fresnel;
                half _Eclat;
                half _Brillance;
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
                half fog : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 n = normalize(i.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                float3 r = reflect(-v, n);
                half3 env = SAMPLE_TEXTURECUBE(_Environnement, sampler_Environnement, r).rgb;
                half fres = pow(1.0h - saturate(dot(n, v)), 4.0h);
                // Métal : le reflet est teinté par la couleur du métal
                half3 c = _Teinte.rgb * (env * _Intensite + _Ambiant);
                c += _Teinte.rgb * fres * _Fresnel;
                Light lune = GetMainLight();
                half spec = pow(saturate(dot(r, lune.direction)), _Brillance) * _Eclat;
                c += lune.color * spec * lerp(_Teinte.rgb, 1.0h, 0.5h);
                c = MixFog(c, i.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}

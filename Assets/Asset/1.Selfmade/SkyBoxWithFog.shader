Shader "SkyboxWithFog"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        [Gamma] _Exposure ("Exposure", Range(0.0, 8.0)) = 1.0
        _Rotation ("Rotation", Range(0.0, 360.0)) = 0.0
        [NoScaleOffset] _Tex ("Cubemap   (HDR)", CUBE) = "grey" {}
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SkyboxWithFog"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _Exposure;
                float _Rotation;
            CBUFFER_END

            TEXTURECUBE(_Tex);
            SAMPLER(sampler_Tex);

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 directionWS : TEXCOORD0;
                float fogCoord : TEXCOORD1;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                
                // Skybox directions (rotated)
                float3 dir = IN.positionOS;

                // Apply rotation around Y
                float rad = radians(_Rotation);
                float cosR = cos(rad);
                float sinR = sin(rad);
                float3 rotatedDir = float3(
                    cosR * dir.x - sinR * dir.z,
                    dir.y,
                    sinR * dir.x + cosR * dir.z
                );

                OUT.directionWS = normalize(rotatedDir);

                // Convert to clip space
                OUT.positionHCS = TransformObjectToHClip(dir);

                // Compute fog coordinate
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURECUBE(_Tex, sampler_Tex, IN.directionWS);

                // HDR decode (assuming texture is HDR, using exposure)
                texColor.rgb = DecodeHDREnvironment(texColor, _Exposure);

                // Tint
                texColor.rgb *= _Tint.rgb;

                // Apply fog
                float3 finalColor = MixFog(texColor.rgb, IN.fogCoord);

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }

    Fallback Off
}


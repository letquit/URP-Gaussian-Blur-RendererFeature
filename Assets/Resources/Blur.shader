Shader "PostProcessing/Blur"
{
    Properties
    {
        _Spread("Standard Deviation (Spread)", Float) = 0
        _GridSize("Grid Size", Integer) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        CBUFFER_START(UnityPerMaterial)
            int _GridSize;
            float _Spread;
        CBUFFER_END

        float4 GaussianBlur(float2 uv, float2 texelStep)
        {
            int radius = (_GridSize - 1) / 2;
            float sigma = max(_Spread, 0.0001);
            float4 color = 0;
            float weightSum = 0;
            for (int offset = -radius; offset <= radius; ++offset)
            {
                float x = offset / sigma;
                float weight = exp(-0.5 * x * x);
                float2 sampleUV = uv + texelStep * offset;
                color += weight * SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, sampleUV, 0);
                weightSum += weight;
            }
            return color / weightSum;
        }
        ENDHLSL

        Pass
        {
            Name "Horizontal"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragHorizontal

            float4 FragHorizontal(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return GaussianBlur(input.texcoord, float2(_BlitTexture_TexelSize.x, 0));
            }
            ENDHLSL
        }

        Pass
        {
            Name "Vertical"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragVertical

            float4 FragVertical(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return GaussianBlur(input.texcoord, float2(0, _BlitTexture_TexelSize.y));
            }
            ENDHLSL
        }
    }
}

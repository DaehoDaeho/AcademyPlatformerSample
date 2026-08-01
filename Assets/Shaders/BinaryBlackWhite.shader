Shader "Hidden/Academy/BinaryBlackWhite"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Binary Black White"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float BinaryEffectEnabled;
            float BinaryThreshold;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 originalColor = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord);

                if (BinaryEffectEnabled < 0.5)
                {
                    return originalColor;
                }

                half luminance = dot(originalColor.rgb, half3(0.2126, 0.7152, 0.0722));
                half binaryValue = step(BinaryThreshold, luminance);
                return half4(binaryValue, binaryValue, binaryValue, originalColor.a);
            }
            ENDHLSL
        }
    }
}

Shader "Custom/ZED2iDepth"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite On

        Pass
        {
            Name "DepthMetric"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float  eyeDepth   : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vpi.positionCS;
                OUT.eyeDepth = -vpi.positionVS.z;
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
                float d = IN.eyeDepth;
                if (d < 0.2 || d > 20.0) return 0.0;
                return d;
            }
            ENDHLSL
        }
    }
}
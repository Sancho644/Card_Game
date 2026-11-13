Shader "CardGame/CardHandDepth"
{
    Properties
    {
        [HideInInspector] _ZWrite("Depth write", Float) = 1
        [HideInInspector] _ColorMask("Color channels", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            ZWrite [_ZWrite]
            ColorMask [_ColorMask]
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float3 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; };
            Varyings Vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS); return o; }
            half4 Frag(Varyings i):SV_Target { return 0; }
            ENDHLSL
        }
    }
}

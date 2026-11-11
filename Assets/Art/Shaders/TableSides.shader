Shader "CardGame/TableSides"
{
    Properties
    {
        _PlayerTint("Player side",Color)=(.10,.65,.52,.10)
        _OpponentTint("Opponent side",Color)=(.65,.42,.22,.10)
        _Divider("Centre divider",Color)=(.85,.66,.35,.65)
        _LineWidth("Line width in UV",Range(.0005,.01))=.0015
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _PlayerTint,_OpponentTint,_Divider;float _LineWidth;
            CBUFFER_END
            struct Attributes {float3 positionOS:POSITION;float2 uv:TEXCOORD0;};
            struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            Varyings Vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS);o.uv=v.uv;return o;}
            half4 Frag(Varyings i):SV_Target
            {
                half4 tint=lerp(_PlayerTint,_OpponentTint,smoothstep(.48,.52,i.uv.y));
                float stripe=1-smoothstep(_LineWidth,_LineWidth*2,abs(i.uv.y-.5));
                float fade=smoothstep(0,.03,i.uv.x)*smoothstep(0,.03,1-i.uv.x);
                return lerp(tint,_Divider,stripe*fade);
            }
            ENDHLSL
        }
    }
}

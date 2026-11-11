Shader "CardGame/CardEdgeGlow"
{
    Properties { _Tint("Tint",Color)=(.2,1,.8,1) _EffectTime("Clock",Float)=0 _Periods("Pulse / Travel",Vector)=(1.6,2.4,0,0) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
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
            half4 _Tint; float _EffectTime; float4 _Periods;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS);o.uv=v.uv;return o;}
            half4 Frag(Varyings i):SV_Target
            {
                float pulse=.5+.5*sin(_EffectTime*6.283185/max(.1,_Periods.x));
                float run=frac(i.uv.x-_EffectTime/max(.1,_Periods.y));
                float glint=pow(saturate(1-abs(run-.5)*13),2);
                float edge=pow(saturate(1-abs(i.uv.y*2-1)),.65);
                return half4(lerp(_Tint.rgb,half3(1,.96,.8),glint*.8),edge*_Tint.a*(.30+.35*pulse+.35*glint));
            }
            ENDHLSL
        }
    }
}

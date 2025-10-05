Shader "Custom/TextureAtlasUnlit"
{
    Properties
    {
        _MainTex("Texture Atlas", 2D) = "white" {}
        _UVMin("UV Min (x,y)", Vector) = (0,0,0,0)
        _UVMax("UV Max (x,y)", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _UVMin;   // (uMin, vMin)
            float4 _UVMax;   // (uMax, vMax)

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                
                // Scale UV to selected region
                float2 uv = IN.uv;
                OUT.uv = lerp(_UVMin.xy, _UVMax.xy, uv);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return tex2D(_MainTex, IN.uv);
            }
            ENDHLSL
        }
    }
}

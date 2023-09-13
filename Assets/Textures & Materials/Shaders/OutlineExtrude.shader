Shader "Unlit/OutlineExtrude"
{
    Properties
    {
		_Color("Color", Color) = (0, 0, 0, 1)
        _Width("Width", float) = 0.03
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline"
        "RenderType"="Opaque"
		"Queue"="Geometry" }
        
        Cull Front

        HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _Color;
        half _Width;
		CBUFFER_END


        struct Attributes {
            float4 positionOS   : POSITION;
            float4 normalOS     : NORMAL;
            float4 tangentOS    : TANGENT;
        };

        struct Varyings {
            float4 positionCS   : SV_POSITION;
            float3 normalWS     : NORMAL;
            float3 positionWS   : TEXCOORD1;
            float3 positionOS   : TEXCOORD2;
            float3 data         : TEXCOORD3;
        };

		ENDHLSL

        Name "ForwardLit"
        Tags { "LightMode"="UniversalForward" }

        pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes IN){
                Varyings OUT;

                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInputs.normalWS;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                // float4 a = mul(unity_WorldToObject, float4(OUT.normalWS,0));
                // OUT.positionCS = OUT.positionCS + (a * _Width);
                OUT.positionWS = positionInputs.positionWS + (OUT.normalWS * _Width);
                // world space to clip space -> view and proj
                OUT.positionCS = mul(UNITY_MATRIX_VP, OUT.positionWS) + OUT.positionCS.w;
                OUT.data = OUT.positionCS;
                OUT.positionCS = mul(UNITY_MATRIX_MVP, IN.positionOS);
                OUT.data = OUT.positionCS;
                float4 b = float4((OUT.normalWS),0) * _Width;
                OUT.positionCS = mul(UNITY_MATRIX_VP, (mul(UNITY_MATRIX_M, IN.positionOS) + b));
                OUT.positionOS = IN.positionOS.xyz;
                
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target {
                //return float4(IN.data,1);
                return _Color;
            }
            
            ENDHLSL
        }
    }
}

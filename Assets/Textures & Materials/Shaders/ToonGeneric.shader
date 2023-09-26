Shader "Custom/ToonGeneric"
{
    Properties
    {
		_BaseColor("Base Colour", Color) = (0.9622642, 0.4336322, 0.27597, 1)
        _ShadedColor("Shaded Colour", Color) = (0.9, 0.38, 0.21, 1)
        _HighlightColor("Highlight Colour", Color) = (1,1,1,1)
        _ShadowDarkness("Shadow Darkness [0,1]", float) = 0.7
        _SpecCoef("Specular Amount", float) = 0.06
        _SpecPower("Specular Power", float) = 20
        _ShadingThreshold("Shading Threshold [0,1]", float) = 0.5
        _HighlightThreshold("Highlight Threshold [0,1] (above shading thresh)", float) = 0.92
        _ShadowThreshold("Shadow Threshold [0+]", float) = 0.3
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline"
        "RenderType"="Opaque"
		"Queue"="Geometry" }
        
        HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseColor;
        float4 _ShadedColor;
        float4 _HighlightColor;
        float _ShadowDarkness;
        float _SpecCoef;
        float _SpecPower;
        float _ShadingThreshold;
        float _HighlightThreshold;
        half _ShadowThreshold;
		CBUFFER_END

        struct Attributes {
            float4 positionOS   : POSITION;
            float4 normalOS     : NORMAL;
            float4 tangentOS    : TANGENT;
            float2 uv           : TEXCOORD0;
            float4 color        : COLOR;
        };

        struct Varyings {
            float4 positionCS   : SV_POSITION;
            float2 uv           : TEXCOORD0;
            float4 color        : COLOR;
            float3 normalWS     : NORMAL;
            float3 positionWS   : TEXCOORD1;
            float3 positionOS   : TEXCOORD2;
            float3 halfWayVec   : TEXCOORD3;//can remove if not work
            float4 shadowCoord  : TEXCOORD4;
        };

		ENDHLSL

        Name "ForwardLit"
        Tags { "LightMode"="UniversalForward" }

        pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ MAIN_LIGHT_CALCULATE_SHADOWS
			#pragma multi_compile _ REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR

            Varyings vert(Attributes IN){
                Varyings OUT;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.color = IN.color;
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                Light light = GetMainLight();
                OUT.halfWayVec = normalize(light.direction + normalize(GetWorldSpaceViewDir(OUT.positionWS)));

                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInputs.normalWS;

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target {
                // Shadows
                float4 shadowCoord;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    shadowCoord = IN.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    shadowCoord = TransformWorldToShadowCoord(float4(IN.positionWS,1));
                #elif defined(_MAIN_LIGHT_SHADOWS)
                    shadowCoord = IN.shadowCoord;
                #else
                    shadowCoord = float4(0, 0, 0, 0);
                #endif
                
                #if defined(_MAIN_LIGHT_SHADOWS)
                    Light light = GetMainLight(shadowCoord);
                #else
                    Light light = GetMainLight();
                #endif
                half totalShadow = 1 - light.shadowAttenuation;
                
                // Main lighting
                float diffComp = saturate(dot(light.direction, IN.normalWS)) * (totalShadow + _ShadowDarkness) * light.color;
                //float3 halfWayVec = normalize(GetWorldSpaceViewDir(IN.positionWS) + light.direction); // light + view
                float specComp = saturate(dot(normalize(IN.halfWayVec), normalize(IN.normalWS)));//try other spaces if it doesn't work
                specComp = pow(specComp, _SpecPower) * _SpecCoef;

                half extraShadow = 0;
                // Additional lighting //https://learnopengl.com/Lighting/Multiple-lights helpful !
                #ifdef _ADDITIONAL_LIGHTS
                    uint additionalLightsCount = GetAdditionalLightsCount();
                    totalShadow /= additionalLightsCount;
                    for (uint i = 0; i < additionalLightsCount; i++){
                        Light extraLight = GetAdditionalLight(i, IN.positionWS);
                        half3 halfWayVec = normalize(normalize(extraLight.direction + GetWorldSpaceViewDir(IN.positionWS)));
                        //specComp += pow(max(dot((halfWayVec), IN.normalWS),0), _SpecPower) * _SpecCoef * ( extraLight.color * extraLight.distanceAttenuation);
                        specComp += pow(max(dot((halfWayVec), IN.normalWS) * ( extraLight.color * extraLight.distanceAttenuation),0), _SpecPower) * _SpecCoef;
                        #ifdef _ADDITIONAL_LIGHT_SHADOWS
                            diffComp += saturate(dot(extraLight.direction, IN.normalWS)) * 
                                (extraLight.shadowAttenuation * extraLight.color * extraLight.distanceAttenuation); // can use = sat(dif) for a clamped result. 
                            totalShadow += (1 - extraLight.shadowAttenuation);
                        #else
                            diffComp += dot(extraLight.direction, IN.normalWS) * extraLight.color * extraLight.distanceAttenuation;//diffComp = saturate(diffComp + dot(extraLight.direction, normal) * _DiffuseCoef * extraLight.color);
                        #endif
                    }
                #endif

                //float4 phongOut = (outColor * diffComp + (outColor * specComp) + (outColor * _AmbientCoef));
                //half lightingPower = diffComp  + _AmbientCoef + specComp;
                bool notInShadow = true;//totalShadow < _ShadowThreshold;
                half lightingPower = notInShadow ? diffComp + specComp : 0.1;
                float4 toonShading = (lightingPower > _ShadingThreshold) ? _BaseColor : _ShadedColor;
                toonShading = (specComp > _HighlightThreshold && notInShadow) ? _HighlightColor : toonShading;

                return toonShading;
                //return float4(a,a,a,1); // useful debug tool
            }

            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}

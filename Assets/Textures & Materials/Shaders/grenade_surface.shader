Shader "Custom/Grenade_Surface"
{
    Properties
    {
		_BaseColor("Base Colour", Color) = (0.9622642, 0.9622642, 0.9622642, 1)
        _HighlightColor("Highlight Colour", Color) = (1,1,1,1)
        _HighlightIntensity("Highlight Intensity", float) = 2
        _Completeness("Completeness [0,1]", float) = 1
        _GridSize("Grid size (integer)", float) = 16
        _MinLighting("Minimum lighting [0,1]", float) = 0.3
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
        float4 _HighlightColor;
        float _HighlightIntensity;
        float _Completeness;
        float _GridSize;
        half _MinLighting;
		CBUFFER_END

        struct AttributesSchnoz {
            float4 positionOS   : POSITION;
            float4 normalOS     : NORMAL;
            float4 tangentOS    : TANGENT;
            float2 uv           : TEXCOORD0;
            float4 color        : COLOR;
        };

        struct VaryingsSchnoz {
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

        pass {
            Name "SchnozToon"
            Tags { "LightMode"="UniversalForward" }
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

            #define PI 3.141592653589793

            float map(float val, float amin, float amax, float bmin, float bmax) {
                return (val - amin) * (bmax - bmin) / (amax - amin) + bmin;
            }

            VaryingsSchnoz vert(AttributesSchnoz IN){
                VaryingsSchnoz OUT;

                //_Completeness = _CosTime.y * 0.5 + 0.5; // demo test the transformation. remove later
                if (IN.positionOS.y > _Completeness) {
                    float difference = abs(IN.positionOS.y - _Completeness);
                    //IN.positionOS.y = _Completeness;
                    //IN.positionOS.xz *= _CosTime.x * 0.5 + 0.5;
                    //IN.positionOS.xz *= tan(_Time.y) * 0.5 + 0.5;
                    float o = abs(IN.positionOS.y - 0.5); // this value is correct
                    //float a = o / tan(_Time.y);
                    //float a = o / tan((asin(o)));
                    //float a = sqrt(1 - pow(o,2)); // best result from trig

                    //float theta = asin(o/0.5);
                    //float a = 0.5 * cos(theta);

                    //float a = 0.5*cos(PI  * o);
                    //float a = 0.25*cos(2 * PI  * o) + 0.25;

                    float a = 1 - difference;

                    IN.positionOS.xz *= a;
                    IN.positionOS.y = _Completeness;
                    // IN.positionOS.xz = a;
                }

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.color = IN.color;
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                Light light = GetMainLight();
                OUT.halfWayVec = normalize(light.direction + normalize(GetWorldSpaceViewDir(OUT.positionWS)));
                //float3 forcedViewDir = GetCurrentViewPosition() - OUT.positionWS;
                //OUT.halfWayVec = normalize(light.direction + forcedViewDir); // light + view

                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInputs.normalWS;

                return OUT;
            }

            float4 frag(VaryingsSchnoz IN) : SV_Target {
                //vec3 finalCol = (floor(mod(gl_FragCoord.x, modGoal)) + floor(mod(gl_FragCoord.y, modGoal))) == 0.0 ? col1 : col2;
                //float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(x * 0.25, y * 0.25) + (IN.positionCS.xy * _MainTex_TexelSize.xy * _MainTex_ST.xy) % 0.25);
                //float result = mod(floor(gl_FragCoord.x * float(gridSize) / pow(float(gridSize), 2.0)) + floor(gl_FragCoord.y * float(gridSize) / pow(float(gridSize), 2.0)), 2.0);

                float4 c = _BaseColor;
                // lighting
                //_BaseColor *= map(dot(IN.normalWS, normalize(_WorldSpaceCameraPos - IN.positionWS)), 0,1, _MinLighting, 1);
                //c = (floor(fmod(IN.positionCS.x, _GridSize)) + floor(fmod(IN.positionCS.x, _GridSize))) == 0.0 ? _BaseColor : _HighlightColor;
                float a = fmod(floor(IN.positionCS.x * float(_GridSize) / pow(float(_GridSize), 2.0)) + floor(IN.positionCS.y * float(_GridSize) / pow(float(_GridSize), 2.0)), 2.0);
                //float a = (fmod(IN.positionCS.x, _GridSize));
                c = a == 0.0 ? _BaseColor : _HighlightColor * _HighlightIntensity;
                c *= map(dot(IN.normalWS, normalize(_WorldSpaceCameraPos - IN.positionWS)), 0,1, _MinLighting, 1);
                return c;
                float4 b = float4(a,a,a,1);
                return b;
                return c;

                // // Shadows
                // float4 shadowCoord;
                // #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                //     shadowCoord = IN.shadowCoord;
                // #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                //     shadowCoord = TransformWorldToShadowCoord(float4(IN.positionWS,1));
                // #elif defined(_MAIN_LIGHT_SHADOWS)
                //     shadowCoord = IN.shadowCoord;
                // #else
                //     shadowCoord = float4(0, 0, 0, 0);
                // #endif
                
                // #if defined(_MAIN_LIGHT_SHADOWS)
                //     Light light = GetMainLight(shadowCoord);
                // #else
                //     Light light = GetMainLight();
                // #endif
                // half totalShadow = 1 - light.shadowAttenuation;
                
                // // Main lighting
                // float diffComp = saturate(dot(light.direction, IN.normalWS) * _DiffuseCoef) * (totalShadow + _ShadowDarkness) * light.color;
                // //float3 halfWayVec = normalize(GetWorldSpaceViewDir(IN.positionWS) + light.direction); // light + view
                // float specComp = saturate(dot(normalize(IN.halfWayVec), normalize(IN.normalWS)));//try other spaces if it doesn't work
                // specComp = pow(specComp, _SpecPower) * _SpecCoef;

                // half extraShadow = 0;
                // // Additional lighting //https://learnopengl.com/Lighting/Multiple-lights helpful !
                // #ifdef _ADDITIONAL_LIGHTS
                //     uint additionalLightsCount = GetAdditionalLightsCount();
                //     totalShadow /= additionalLightsCount;
                //     for (uint i = 0; i < additionalLightsCount; i++){
                //         Light extraLight = GetAdditionalLight(i, IN.positionWS);
                //         half3 halfWayVec = normalize(normalize(extraLight.direction + GetWorldSpaceViewDir(IN.positionWS)));
                //         //specComp += pow(max(dot((halfWayVec), IN.normalWS),0), _SpecPower) * _SpecCoef * ( extraLight.color * extraLight.distanceAttenuation);
                //         specComp += pow(max(dot((halfWayVec), IN.normalWS) * ( extraLight.color * extraLight.distanceAttenuation),0), _SpecPower) * _SpecCoef;
                //         #ifdef _ADDITIONAL_LIGHT_SHADOWS
                //             diffComp += saturate(dot(extraLight.direction, IN.normalWS) * _DiffuseCoef) * 
                //                 (extraLight.shadowAttenuation * extraLight.color * extraLight.distanceAttenuation); // can use = sat(dif) for a clamped result. 
                //             totalShadow += (1 - extraLight.shadowAttenuation);
                //         #else
                //             diffComp += dot(extraLight.direction, IN.normalWS) * _DiffuseCoef * extraLight.color * extraLight.distanceAttenuation;//diffComp = saturate(diffComp + dot(extraLight.direction, normal) * _DiffuseCoef * extraLight.color);
                //         #endif
                //     }
                // #endif

                // //float4 phongOut = (outColor * diffComp + (outColor * specComp) + (outColor * _AmbientCoef));
                // bool notInShadow = true;//totalShadow < _ShadowThreshold;
                // half lightingPower = notInShadow ? diffComp  + _AmbientCoef + specComp : 0.1;
                // float4 toonShading = (lightingPower > _ShadingThreshold) ? _BaseColor : _ShadedColor;
                // toonShading = lerp(_IrritatedColor, toonShading, saturate(distToIrrit / _Blending));
                // toonShading = lerp((lightingPower > _ShadingThreshold) ? _HoldingBreathColor : _HoldingBreathShadedColor, toonShading, saturate(distToHold / _Blending));
                // toonShading = (specComp > _HighlightThreshold && notInShadow) ? lerp(_HoldingBreathHighlightColor, _HighlightColor,saturate(distToHold / _Blending)) : toonShading;
                // return toonShading; // originally phongout
                // //return float4(a,a,a,1); // useful debug tool
            }

            ENDHLSL
        }
        // UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}

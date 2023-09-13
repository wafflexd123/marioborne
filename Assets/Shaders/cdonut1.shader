Shader "Custom/cdonut"
{
    Properties
    {
		_TextColor("Text Colour", Color) = (0.9622642, 0.4336322, 0.27597, 1)
		_BackgroundColor("Background Colour", Color) = (0.622642, 0.336322, 0.14597, 1)
        _ShadowDarkness("Shadow Darkness [0,1]", float) = 0.7
        _AmbiCoef("Ambient Amount", float) = 0.06
        _DiffCoef("Diffuse Amount", float) = 0.06
        _SpecCoef("Specular Amount", float) = 0.06
        _SpecPower("Specular Power", float) = 20
        _MainTex("Textual lighting texture",2D) = "white" {}
        _NumChars("Number of characters in the map", float) = 16.0
        _UseGooch("Use gooch shading [0/1]", float) = 1
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
		float4 _TextColor;
		float4 _BackgroundColor;
        float _ShadowDarkness;
        float _AmbiCoef;
        float _DiffCoef;
        float _SpecCoef;
        float _SpecPower;
        float4 _MainTex_ST;
        float4 _MainTex_TexelSize;
        half _NumChars;
        half _UseGooch;
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
            float3 positionNDC   : TEXCOORD3;
            float3 halfWayVec   : TEXCOORD4;//can remove if not work
            float4 shadowCoord  : TEXCOORD5;
        };
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

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

            float GetLumi(float4 c){
                return 0.2126*c.r + 0.7152*c.g + 0.0722*c.b;
            }

            Varyings vert(Attributes IN){
                Varyings OUT;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.positionNDC = positionInputs.positionNDC;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.color = IN.color;
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                Light light = GetMainLight();
                OUT.halfWayVec = normalize(light.direction + normalize(GetWorldSpaceViewDir(OUT.positionWS)));

                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInputs.normalWS;

                float4 NDC = mul(IN.positionOS.xyz, UNITY_MATRIX_MV);
                OUT.positionNDC = NDC;
                OUT.positionNDC.xy /= NDC.w;
                //OUT.positionNDC.xy /= positionInputs.positionNDC.w;

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
                float diffComp = (_UseGooch > 0.98) ? saturate((dot(light.direction, IN.normalWS) + 1) / 2) : saturate(dot(light.direction, IN.normalWS));
                diffComp *= (totalShadow + _ShadowDarkness) * light.color;
                //float3 halfWayVec = normalize(GetWorldSpaceViewDir(IN.positionWS) + light.direction); // light + view
                float specComp = saturate(dot(normalize(IN.halfWayVec), normalize(IN.normalWS)));//try other spaces if it doesn't work
                specComp = pow(specComp, _SpecPower) * _SpecCoef;

                float totalLighting = clamp(_AmbiCoef + (diffComp * _DiffCoef) + specComp,0.0,1.0);

                // find the uv to read the texture
                int index = floor(totalLighting * _NumChars) - 1;
                int sq = sqrt(_NumChars);
                int y = index / sq;
                int x = index % sq;
                //return float4(x*0.25,y*0.25,0,1);

                float p = (x + (y*4.0)) / 16.0;
                //return float4(p,p,p, 1.0);

                // read the texture
                // if (floor(IN.positionCS.x) == 80.0)
                //     return float4(1,0.5,0,1);
                // return float4(IN.positionCS.x % sq, IN.positionCS.y % 4, 0.0, 1.0); 
                //float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.positionCS.x % _MainTex_TexelSize.z, IN.positionCS.y % _MainTex_TexelSize.w));
                //float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.positionCS.x * _MainTex_TexelSize.x * 10.0, IN.positionCS.y * _MainTex_TexelSize.y * 10.0));
                // float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2((IN.positionCS.x * _MainTex_TexelSize.x + x) * 10.0, (IN.positionCS.y * _MainTex_TexelSize.y + y) * 10.0)); // pretty fucked up but kind of interesting
                //float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2((IN.positionCS.x) * _MainTex_TexelSize.x + x * 0.25, (IN.positionCS.y) * _MainTex_TexelSize.y + y * 0.25));
                
                //float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(x * 0.25, y * 0.25) + (IN.positionCS.xy * _MainTex_TexelSize.xy) % 0.25); // first correct results. suffering from terrible artifacts on complex geom
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(x * 0.25, y * 0.25) + (IN.positionCS.xy * _MainTex_TexelSize.xy * _MainTex_ST.xy) % 0.25); // first correct results. suffering from terrible artifacts on complex geom
                //return float4(float2(x * 0.25, y * 0.25) + (IN.positionCS.xy * _MainTex_TexelSize.xy) % 0.25,0,1);

                // this is good use this in final
                tex = GetLumi(tex) > 0.5 ? _TextColor : _BackgroundColor; 
                return tex;

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
                // bool notInShadow = true;//totalShadow < _ShadowThreshold;
                // half lightingPower = notInShadow ? diffComp + specComp : 0.1;
                // float4 toonShading = (lightingPower > _ShadingThreshold) ? _BaseColor : _ShadedColor;
                // toonShading = (specComp > _HighlightThreshold && notInShadow) ? _HighlightColor : toonShading;

                

                // if (floor(IN.positionNDC.x * _ScreenParams.x) == 50.0)
                //     return float4(1,1,0,1);
                // if (floor(IN.positionNDC.y * _ScreenParams.y) == 50.0)
                //     return float4(0,1,1,1);

                // return float4(IN.positionNDC.x / _ScreenParams.x, IN.positionNDC.y / _ScreenParams.y, 0.0, 1.0);
                // return float4(IN.positionNDC.x, IN.positionNDC.y, 0.0, 1.0);
                // return float4(IN.positionNDC.x / 100.0, IN.positionNDC.y / 100.0, 0.0, 1.0);
                return totalLighting;
                //return float4(a,a,a,1); // useful debug tool
            }

            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}

Shader "Arielado/SkyboxTest" {
    SubShader {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass {
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/HLSL/Include/Math.hlsl"
            #include "Assets/Shaders/HLSL/Include/GlobalInputs.hlsl"
            #include "Assets/Shaders/HLSL/Include/GlobalConstants.hlsl"
            

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float2 uv : TEXCOORD2;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 getValFromSkyLUTCoords(float3 rayDir) {
                float3 ground = float3(0, _PLANET_RADIUS+_GROUND_EPSILON, 0);
                float3 camPos = GetCameraPositionWS()*_VIEW_POSITION_SCALE;
                float3 viewPos = ground + camPos;

                float height = length(viewPos);
                float3 up = viewPos / height;
                
                //float horizonAngle = safeacos(sqrt(height * height - _PLANET_RADIUS * _PLANET_RADIUS) / height);
                float horizonAngle = acos(sqrt(height * height - _PLANET_RADIUS * _PLANET_RADIUS) / height);
                float altitudeAngle = horizonAngle - acos(dot(rayDir, up)); // Between -PI/2 and PI/2
                float azimuthAngle; // Between 0 and 2*PI 
            
                float3 right = float3(1, 0, 0);
                float3 forward = float3(0, 0, -1);
                
                float sinTheta = dot(rayDir, right);
                float cosTheta = dot(rayDir, forward);
                
                azimuthAngle = atan2(sinTheta, cosTheta) + PI;
                
                // Non-linear mapping of altitude angle. See Section 5.3 of the paper.
                float v = 0.5 + 0.5*sign(altitudeAngle)*sqrt(abs(altitudeAngle)*2/PI);
                float2 uv = float2(azimuthAngle / PI*0.5, v);
                
                return uv;
            }

            Varyings vert(Attributes IN) {
                Varyings OUT = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(IN);           
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                float2 screenUV = (IN.screenPos.xy / IN.screenPos.w);
                screenUV = screenUV * _ScreenParams.xy;
                
                float3 viewPos = GetCameraPositionWS();
                float3 viewDirWS = normalize(IN.posWS - viewPos);
                
                float2 skyViewUV = getValFromSkyLUTCoords(viewDirWS);
                
                half3 result = SAMPLE_TEXTURE2D_LOD(_SKY_VIEW_LUT, sampler_SKY_VIEW_LUT, skyViewUV, 0).rgb;
                half dither = SAMPLE_TEXTURE2D_LOD(_DITHER, sampler_DITHER, screenUV * 0.75, 0).r;

                half colorMagnitude = (result.x + result.y + result.z) * 0.33333333;
                result += result * dither * 0.75 * (1 - colorMagnitude);

                return half4(result, 1);
            }
            ENDHLSL
        }
    }
}

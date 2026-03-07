using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Arielado.Lighting.Models;

#if UNITY_EDITOR
    using UnityEditor;
    using Arielado.EditorTools;
#endif

namespace Arielado.Lighting {
    [ExecuteAlways]
    public class AtmosphereRenderer : MonoBehaviour {
        [SerializeField] private Light sun;
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private AtmosphereParametersSO parameters;
        [SerializeField] private AtmosphereLUTParameters LUTParameters;
        [SerializeField] private RenderTextureFormat colorFormat;
        [SerializeField] private Texture2D ditherPattern;
        [SerializeField] private RenderTexture stLUT, msLUT, skyViewLUT;

        private ComputeBuffer atmosphereParametersBuffer, lutParametersBuffer;
        [SerializeField, ReadOnly] private Vector2Int 
                                        sunTThreadGroups    = new Vector2Int(256, 64),
                                        msThreadGroups      = new Vector2Int(32, 32), 
                                        skyViewThreadGroups = new Vector2Int(128, 128);

        #if UNITY_EDITOR
            AtmosphereParametersSO previousParameters;
        #endif

        private void Awake() {
            Setup();
        }

        
        private void OnEnable() {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            
            if (sun == null) sun = FindFirstObjectByType<Light>();
            if (sun == null || sun.type != LightType.Directional) Debug.LogWarning("Please assign a directional light in the inspector.");

            #if UNITY_EDITOR
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

                ShaderChangedDispatcher.OnComputeShaderChanged += ReloadComputeShader;

                parameters.onDataChange += Setup;
                previousParameters = parameters;
            #endif
        }

        private void OnDisable() {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

            #if UNITY_EDITOR
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

                ShaderChangedDispatcher.OnComputeShaderChanged -= ReloadComputeShader;
                    
                parameters.onDataChange -= Setup;
            #endif
        }   
        
        #if UNITY_EDITOR
            private void ReloadComputeShader(ComputeShader computeShader) {
                if (computeShader != this.computeShader) return;

                Setup();
            }
            private void OnBeforeAssemblyReload() {
                CleanUp();
            }

            private void OnAfterAssemblyReload() {
                Setup();
            }

            private void OnValidate() {
                if (previousParameters != parameters) {
                    if (previousParameters != null) previousParameters.onDataChange -= Setup;
                    if (parameters != null) parameters.onDataChange += Setup;
                }

                if (sun == null) sun = FindFirstObjectByType<Light>();
                if (sun == null || sun.type != LightType.Directional) Debug.LogWarning("Please assign a directional light in the inspector.");

                previousParameters = parameters;

                Setup();
            }
        #endif

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) {
            UpdateSkyViewLUT(camera);
        }

        private void OnDestroy() {
            CleanUp();
        }
           

        private void CleanUp() {
            if (atmosphereParametersBuffer != null) { atmosphereParametersBuffer?.Dispose(); atmosphereParametersBuffer = null; }
            if (lutParametersBuffer!= null) { lutParametersBuffer?.Dispose(); lutParametersBuffer = null; }

            if (stLUT != null) { stLUT?.Release(); stLUT = null; }
            if (msLUT != null) { msLUT?.Release(); msLUT = null; }
            if (skyViewLUT != null) { skyViewLUT.Release(); skyViewLUT = null; }
        }

        private void UpdateSkyViewLUT(Camera camera) {
            if (sun == null) sun = FindFirstObjectByType<Light>();
            if (sun == null) return;

            Vector4 viewPos = camera.transform.position;
            viewPos.w = 1;

            Vector3 sunDir = -sun.transform.forward;
        
            computeShader.SetVector(ShaderProperties._VIEW_POR_BAKER_ID, viewPos);
            computeShader.SetVector(ShaderProperties._SUN_DIR_BAKER_ID, new Vector4(sunDir.x, sunDir.y, sunDir.z, sun.intensity));
            computeShader.Dispatch(2, skyViewThreadGroups.x, skyViewThreadGroups.y, 1);
        }

        private void Setup() {
            CleanUp();

            SetupBuffers();
            SetupSTLUT();
            SetupMSLUT();
            SetupSkyViewLUT();
            
            BakeSTLUT();
            BakeMSLUT();
        } 
        private void SetupBuffers() {
            AtmosphereParametersGPUAligned parametersGPUAligned = AtmosphereParameters.GetGPUAlignedData(parameters.Parameters);
            
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(parametersGPUAligned);
            atmosphereParametersBuffer = atmosphereParametersBuffer == null ? new ComputeBuffer(1, stride, ComputeBufferType.Constant) : atmosphereParametersBuffer;
            atmosphereParametersBuffer.SetData(new[] { parametersGPUAligned });

            stride = System.Runtime.InteropServices.Marshal.SizeOf(LUTParameters);
            lutParametersBuffer = lutParametersBuffer == null ? new ComputeBuffer(1, stride, ComputeBufferType.Constant) : lutParametersBuffer;
            lutParametersBuffer.SetData(new[] { LUTParameters });

            computeShader.SetConstantBuffer(ShaderProperties._ATMOSPHERE_PARAMETERS_ID, atmosphereParametersBuffer, 0, atmosphereParametersBuffer.stride);
            computeShader.SetConstantBuffer(ShaderProperties._ATMOSPHERE_LUT_PARAMETERS_ID, lutParametersBuffer, 0, lutParametersBuffer.stride);

            Shader.SetGlobalTexture(ShaderProperties._DITHER_ID, ditherPattern);
            Shader.SetGlobalFloat(ShaderProperties._PLANET_RADIUS_ID ,parameters.PlanetRadius);
        }

        private void SetupSTLUT() {
            stLUT = CreateLUT(LUTParameters.sTLUTRes.x, LUTParameters.sTLUTRes.y, colorFormat, TextureWrapMode.Clamp, false, 0);

            computeShader.SetTexture(0, ShaderProperties._SUN_TRANSMITTANCE_LUT_ID, stLUT);
            computeShader.SetTexture(1, ShaderProperties._SUN_TRANSMITTANCE_LUT_ID, stLUT);
            computeShader.SetTexture(2, ShaderProperties._SUN_TRANSMITTANCE_LUT_ID, stLUT);
            
            computeShader.GetKernelThreadGroupSizes(0, out uint x, out uint y, out _);
            sunTThreadGroups = new Vector2Int(stLUT.width / (int)x, stLUT.height / (int)y);
        }

        private void SetupMSLUT() {
            msLUT = CreateLUT(LUTParameters.mSLUTRes.x, LUTParameters.mSLUTRes.y, colorFormat, TextureWrapMode.Clamp, false, 0);

            computeShader.SetTexture(1, ShaderProperties._MULTI_SCATTERING_LUT_ID, msLUT);
            computeShader.SetTexture(2, ShaderProperties._MULTI_SCATTERING_LUT_ID, msLUT);
            
            computeShader.GetKernelThreadGroupSizes(1, out uint x, out uint y, out _);
            msThreadGroups = new Vector2Int(msLUT.width / (int)x, msLUT.height / (int)y);
        }

        private void SetupSkyViewLUT() {
            skyViewLUT = CreateLUT(LUTParameters.skyViewLUTRes.x, LUTParameters.skyViewLUTRes.y, colorFormat, TextureWrapMode.Clamp, false, 0);

            computeShader.SetTexture(2, ShaderProperties._SKY_VIEW_LUT_BAKER_ID, skyViewLUT);
            Shader.SetGlobalTexture(ShaderProperties._SKY_VIEW_LUT_GLOBAL_ID, skyViewLUT);
            
            computeShader.GetKernelThreadGroupSizes(2, out uint x, out uint y, out _);
            skyViewThreadGroups = new Vector2Int(skyViewLUT.width / (int)x, skyViewLUT.height / (int)y);
        }

        private void BakeSTLUT() =>
            computeShader.Dispatch(0, sunTThreadGroups.x, sunTThreadGroups.y, 1);
        private void BakeMSLUT() =>
            computeShader.Dispatch(1, msThreadGroups.x, msThreadGroups.y, 1);

        private RenderTexture CreateLUT(int _width, int _height, RenderTextureFormat format, TextureWrapMode wrapMode, bool useMipmaps, int mipCount) {
            RenderTextureDescriptor rtDesc = new RenderTextureDescriptor() {
                width = _width,
                height = _height,
                volumeDepth = 1,
                enableRandomWrite = true,  
                depthBufferBits = 0,
                stencilFormat = 0,
                depthStencilFormat = 0,
                msaaSamples = 1,
                useMipMap = useMipmaps,
                autoGenerateMips = useMipmaps,
                mipCount = mipCount,
                colorFormat = format,
                dimension = TextureDimension.Tex2D
            };

            RenderTexture rt = new RenderTexture(rtDesc);
            rt.wrapMode = wrapMode;
            rt.Create();

            return rt;
        } 
    }
}
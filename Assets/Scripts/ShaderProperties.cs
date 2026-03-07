using UnityEngine;

namespace Arielado.Lighting {
    public static class ShaderProperties {
        public const string 
        #region GLOBAL
            _DITHER = "_DITHER",
            _SKY_VIEW_LUT = "_SKY_VIEW_LUT",
            _SUN_DIR = "_SUN_DIR",
            _PLANET_RADIUS = "_PLANET_RADIUS",
        #endregion
        #region CONSTANT BUFFERS
            _AtmosphereParameters = "AtmosphereParameters",
            _AtmosphereLUTParameters = "AtmosphereLUTParameters",
            #endregion
        #region SKYBOX BAKER
            _SunTransmittanceLUT = "_SunTransmittanceLUT",
            _MultiScatteringLUT = "_MultiScatteringLUT",
            _SkyViewLUT = "_SkyViewLUT",
            _SunDir = "_SunDir",
            _ViewPos = "_ViewPos";
        #endregion

        public static int 
        #region GLOBAL 
            _DITHER_ID = Shader.PropertyToID(_DITHER),
            _SKY_VIEW_LUT_GLOBAL_ID = Shader.PropertyToID(_SKY_VIEW_LUT),
            _SUN_DIR_ID = Shader.PropertyToID(_SUN_DIR),
            _PLANET_RADIUS_ID = Shader.PropertyToID(_PLANET_RADIUS),
        #endregion
        #region CONSTANT BUFFERS
            _ATMOSPHERE_PARAMETERS_ID = Shader.PropertyToID(_AtmosphereParameters),
            _ATMOSPHERE_LUT_PARAMETERS_ID = Shader.PropertyToID(_AtmosphereLUTParameters),
        #endregion 
        #region SKYBOX BAKER
            _SUN_TRANSMITTANCE_LUT_ID = Shader.PropertyToID(_SunTransmittanceLUT),
            _MULTI_SCATTERING_LUT_ID = Shader.PropertyToID(_MultiScatteringLUT),
            _SKY_VIEW_LUT_BAKER_ID = Shader.PropertyToID(_SkyViewLUT),
            _SUN_DIR_BAKER_ID = Shader.PropertyToID(_SunDir),
            _VIEW_POR_BAKER_ID = Shader.PropertyToID(_ViewPos);
        #endregion 
    }
}
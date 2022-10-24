using UnityEngine;

namespace ProjectTolerance.Shading
{
    public static class ShaderPropertyId
    {
        public static readonly int VerticalCutAlpha = Shader.PropertyToID("_VerticalCutAlpha");
        public static readonly int AmbientLightMapTexture = Shader.PropertyToID("_AmbientLightMapTexture");
        public static readonly int AmbientLightMapSize = Shader.PropertyToID("_AmbientLightMapSize");
        public static readonly int AmbientLightMapOffset = Shader.PropertyToID("_AmbientLightMapOffset");
        public static readonly int Color = Shader.PropertyToID("_Color");
    }
}
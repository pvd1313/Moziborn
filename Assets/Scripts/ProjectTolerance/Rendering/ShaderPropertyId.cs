using UnityEngine;

namespace ProjectTolerance.Rendering
{
    public static class ShaderPropertyId
    {
        public static readonly int VerticalCutAlpha = Shader.PropertyToID("_VerticalCutAlpha");
        public static readonly int VerticalCutRange = Shader.PropertyToID("_VerticalCutRange");
        
        public static readonly int AmbientLightMapTexture = Shader.PropertyToID("_AmbientLightMapTexture");
        public static readonly int AmbientLightMapWorldSize = Shader.PropertyToID("_AmbientLightMapWorldSize");
        public static readonly int AmbientLightMapOffset = Shader.PropertyToID("_AmbientLightMapOffset");

        public static readonly int TorchLightMapTexture = Shader.PropertyToID("_TorchLightMapTexture");
        public static readonly int TorchLightMapWorldSize = Shader.PropertyToID("_TorchLightMapWorldSize");
        public static readonly int TorchLightMapRotation = Shader.PropertyToID("_TorchLightMapRotation");
        public static readonly int TorchLightMapOffset = Shader.PropertyToID("_TorchLightMapOffset");

        public static readonly int Color = Shader.PropertyToID("_Color");
    }
}
using UnityEngine;

namespace ProjectTolerance.Rendering
{
    public static class ShaderPropertyId
    {
        public static class Ambient
        {
            public static readonly int StateTexture = Shader.PropertyToID("_StateTexture");
            public static readonly int SunTexture = Shader.PropertyToID("_SunTexture");
            public static readonly int WallTexture = Shader.PropertyToID("_WallTexture");
            public static readonly int Blur = Shader.PropertyToID("_Blur");
            public static readonly int BilinearFall = Shader.PropertyToID("_BilinearFall");
            public static readonly int LinearFall = Shader.PropertyToID("_LinearFall");
        }
        
        public static readonly int VerticalCutAlpha = Shader.PropertyToID("_VerticalCutAlpha");
        public static readonly int VerticalCutRange = Shader.PropertyToID("_VerticalCutRange");
        
        public static readonly int AmbientLightMapTexture = Shader.PropertyToID("_AmbientLightMapTexture");
        public static readonly int AmbientLightMapWorldSize = Shader.PropertyToID("_AmbientLightMapWorldSize");
        public static readonly int AmbientLightMapOffset = Shader.PropertyToID("_AmbientLightMapOffset");

        public static readonly int WarFogMapTexture = Shader.PropertyToID("_WarFogMapTexture");

        public static readonly int TorchLightMapTexture = Shader.PropertyToID("_TorchLightMapTexture");
        public static readonly int TorchLightMapWorldSize = Shader.PropertyToID("_TorchLightMapWorldSize");
        public static readonly int TorchLightMapRotation = Shader.PropertyToID("_TorchLightMapRotation");
        public static readonly int TorchLightMapOffset = Shader.PropertyToID("_TorchLightMapOffset");
    }
}
using System;

namespace ProjectTolerance.Shading.Lighting
{
    [Serializable]
    public class TorchLightConfig
    {
        public TorchLightView torchLightViewPrefab;
        public int renderTextureResolution;
    }
}
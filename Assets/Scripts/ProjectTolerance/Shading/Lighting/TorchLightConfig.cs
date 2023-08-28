using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    [Serializable]
    public class TorchLightConfig
    {
        public Shader postShader;
        public TorchLightView torchLightViewPrefab;
        public int renderTextureResolution;
        public List<int> postMatrix;
    }
}
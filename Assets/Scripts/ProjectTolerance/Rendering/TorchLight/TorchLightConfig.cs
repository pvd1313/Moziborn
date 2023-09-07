using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTolerance.Rendering.TorchLight
{
    [Serializable]
    public class TorchLightConfig
    {
        public Shader postShader;
        public Shader torchLightShader;
        public Shader warFogSpriteShader;
        public TorchLightView torchLightViewPrefab;
        public int renderTextureResolution;
        public List<int> postMatrix;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTolerance.Rendering.PointLight
{
    [Serializable]
    public class PointLightRenderConfig
    {
        public Shader postShader;
        public int textureResolution;
        public List<int> postMatrix;
    }
}
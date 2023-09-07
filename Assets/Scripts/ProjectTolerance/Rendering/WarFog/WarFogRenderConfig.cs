using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTolerance.Rendering.WarFog
{
    [Serializable]
    public class WarFogRenderConfig
    {
        public Shader postShader;
        public int textureResolution;
        public List<int> postMatrix;
    }
}
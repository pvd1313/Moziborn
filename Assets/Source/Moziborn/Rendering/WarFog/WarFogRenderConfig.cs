using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moziborn.Rendering.WarFog
{
    [Serializable]
    public class WarFogRenderConfig
    {
        public Shader ambientShader;
        public float linearFall;
        public float bilinearFall;
        public float blur;
    }
}
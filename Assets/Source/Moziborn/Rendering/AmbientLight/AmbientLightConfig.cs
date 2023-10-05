using System;
using UnityEngine;

namespace Moziborn.Rendering.AmbientLight
{
    [Serializable]
    public class AmbientLightConfig
    {
        public Shader ambientLightShader;
        public float linearFall;
        public float bilinearFall;
        public float blur;
    }
}
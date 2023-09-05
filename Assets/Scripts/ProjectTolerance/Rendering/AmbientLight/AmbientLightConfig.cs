using System;
using UnityEngine;

namespace ProjectTolerance.Rendering.AmbientLight
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
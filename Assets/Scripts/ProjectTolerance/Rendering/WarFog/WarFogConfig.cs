using System;
using UnityEngine;

namespace ProjectTolerance.Rendering.WarFog
{
    [Serializable]
    public class WarFogConfig
    {
        public WarFogPixelView warFogPixelViewPrefab;
        public Shader ambientShader;
        public float linearFall;
        public float bilinearFall;
        public float blur;
    }
}
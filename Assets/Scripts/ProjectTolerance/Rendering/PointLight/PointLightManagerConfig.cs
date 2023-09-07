using System;
using UnityEngine;

namespace ProjectTolerance.Rendering.PointLight
{
    [Serializable]
    public class PointLightManagerConfig
    {
        public Shader pointLightShader;
        public Shader warFogLightShader;
        public PointLightView _pointLightViewPrefab;
    }
}
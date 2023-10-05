using System;
using UnityEngine;

namespace Moziborn.Rendering.PointLight
{
    [Serializable]
    public class PointLightManagerConfig
    {
        public Shader pointLightShader;
        public PointLightView _pointLightViewPrefab;
    }
}
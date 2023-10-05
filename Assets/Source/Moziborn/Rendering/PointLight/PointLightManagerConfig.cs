using UnityEngine;

namespace Moziborn.Rendering.PointLight
{
    public class PointLightManagerConfig : ScriptableObject
    {
        public Shader pointLightShader;
        public PointLightView pointLightViewPrefab;
    }
}
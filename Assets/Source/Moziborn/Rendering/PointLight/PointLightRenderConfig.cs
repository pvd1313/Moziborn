using System.Collections.Generic;
using UnityEngine;

namespace Moziborn.Rendering.PointLight
{
    public class PointLightRenderConfig : ScriptableObject
    {
        public Shader postShader;
        public int textureResolution;
        public List<int> postMatrix;
    }
}
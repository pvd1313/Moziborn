using UnityEngine;

namespace Moziborn.Rendering.AmbientLight
{
    public class AmbientLightConfig : ScriptableObject
    {
        public Shader ambientLightShader;
        public float linearFall;
        public float bilinearFall;
        public float blur;
    }
}
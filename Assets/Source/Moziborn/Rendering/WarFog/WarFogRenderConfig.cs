using UnityEngine;

namespace Moziborn.Rendering.WarFog
{
    public class WarFogRenderConfig : ScriptableObject
    {
        public Shader ambientShader;
        public float linearFall;
        public float bilinearFall;
        public float blur;
    }
}
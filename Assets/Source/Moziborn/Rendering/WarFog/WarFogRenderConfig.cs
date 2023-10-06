using UnityEngine;

namespace Moziborn.Rendering.WarFog
{
    public class WarFogRenderConfig : ScriptableObject
    {
        public Shader ambientShader;
        public Shader postMergeShader;
        public Shader postFadeShader;
        public float sampleRate;
        public float ambientLinearFall;
        public float ambientBilinearFall;
        public float ambientBlur;
        public float fadeDelta;
    }
}
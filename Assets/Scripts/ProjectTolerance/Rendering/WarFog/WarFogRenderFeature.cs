using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogRenderFeature : ScriptableRendererFeature
    {
        private WarFogRenderPass _warFogPass;
        
        public override void Create()
        {
            _warFogPass = new WarFogRenderPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_warFogPass);
        }
    }
}
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogRenderPass : ScriptableRenderPass
    {
        private readonly Material _material;
        private readonly int _warFogId;

        private RenderTargetIdentifier _cameraRenderTexture;
        private RenderTargetIdentifier _warFogRenderTexture;
        
        public WarFogRenderPass()
        {
            _material = CoreUtils.CreateEngineMaterial("ProjectTolerance/WarFogPost");
            _warFogId = Shader.PropertyToID("WarFogRenderPass");
            
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _cameraRenderTexture = renderingData.cameraData.renderer.cameraColorTarget;
            _warFogRenderTexture = new RenderTargetIdentifier(_warFogId);
            cmd.GetTemporaryRT(_warFogId, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(nameof(WarFogRenderPass));

            Blit(cmd, _cameraRenderTexture, _warFogRenderTexture, _material);

            Blit(cmd, _warFogRenderTexture, _cameraRenderTexture);

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_warFogId);
        }
    }
}
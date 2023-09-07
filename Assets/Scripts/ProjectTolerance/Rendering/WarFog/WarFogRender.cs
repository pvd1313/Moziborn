using System;
using System.Collections;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Layers;
using ProjectTolerance.Rendering.ScreenSpace;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogRender : IDisposable
    {
        private readonly MainCameraController _mainCamera;
        private readonly WarFogRenderConfig _config;

        private ScreenSpaceRender _renderer;

        public WarFogRender (MainCameraController mainCamera, WarFogRenderConfig config)
        {
            _mainCamera = mainCamera;
            _config = config;
        }
        
        public IEnumerator Initialize()
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;

            _renderer = new ScreenSpaceRender(new ScreenSpaceRenderSetup()
            {
                mainCamera = _mainCamera,
                postMatrix = _config.postMatrix,
                postShader = _config.postShader,
                textureResolution = _config.textureResolution,
                cameraCullingMask = 1 << LayerIndex.WarFogLight,
                textureFilterMode = FilterMode.Point,
                cameraDepth = CameraDepth.WarFog,
                textureFormat = RenderTextureFormat.R16,
                renderName = nameof(WarFogRender)
            });

            yield break;
        }

        private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _renderer.BeginCameraRendering(camera);
        }

        private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _renderer.EndCameraRendering(camera);
            
            Shader.SetGlobalTexture(ShaderPropertyId.WarFogMapTexture, _renderer.lastRenderResult.texture);
            Shader.SetGlobalVector(ShaderPropertyId.WarFogMapRotation, _renderer.lastRenderResult.textureRotationSinCosWorldSpace);
            Shader.SetGlobalVector(ShaderPropertyId.WarFogMapOffset, _renderer.lastRenderResult.textureOffsetWorldSpace);
            Shader.SetGlobalVector(ShaderPropertyId.WarFogMapWorldSize, _renderer.lastRenderResult.textureSizeWorldSpace);
        }

        public void Dispose()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
        }
    }
}
using System;
using System.Collections;
using Moziborn.Cinematics;
using Moziborn.Layers;
using Moziborn.Rendering.ScreenSpace;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moziborn.Rendering.PointLight
{
    public class PointLightRender : IDisposable
    {
        private readonly MainCameraController _mainCamera;
        private readonly PointLightRenderConfig _config;
        
        private ScreenSpaceRender _renderer;

        public PointLightRender (MainCameraController mainCamera, PointLightRenderConfig config)
        {
            _mainCamera = mainCamera;
            _config = config;
        }
        
        public IEnumerator Initialize()
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
            
            _renderer = new ScreenSpaceRender(new ScreenSpaceRenderSetup
            {
                mainCamera = _mainCamera,
                postMatrix = _config.postMatrix,
                postShader = _config.postShader,
                textureResolution = _config.textureResolution,
                cameraCullingMask = 1 << LayerIndex.PointLight,
                textureFilterMode = FilterMode.Point,
                cameraDepth = CameraDepth.PointLight,
                textureFormat = RenderTextureFormat.R16,
                renderName = nameof(PointLightRender)
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

            Shader.SetGlobalTexture(ShaderPropertyId.PointLightMapTexture, _renderer.lastRenderResult.texture);
            Shader.SetGlobalVector(ShaderPropertyId.PointLightMapRotation, _renderer.lastRenderResult.textureRotationSinCosWorldSpace);
            Shader.SetGlobalVector(ShaderPropertyId.PointLightMapOffset, _renderer.lastRenderResult.textureOffsetWorldSpace);
            Shader.SetGlobalVector(ShaderPropertyId.PointLightMapWorldSize, _renderer.lastRenderResult.textureSizeWorldSpace);
        }
        
        public void Dispose()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
        }
    }
}
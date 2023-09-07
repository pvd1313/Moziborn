using System;
using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Layers;
using ProjectTolerance.Rendering.ScreenSpace;
using UnityEngine;
using UnityEngine.Rendering;

using Object = UnityEngine.Object;

namespace ProjectTolerance.Rendering.TorchLight
{
    public class TorchLightController : IDisposable
    {
        private readonly TorchLightConfig _config;
        private readonly MainCameraController _mainCamera;
        
        private Transform _root;
        private Dictionary<int, TorchLightView> _lightViews;
        private int _nextId;
        private Material _torchSpriteMaterial;
        private Material _warFogSpriteMaterial;

        private ScreenSpaceRender _renderer;

        public TorchLightController (TorchLightConfig config, MainCameraController mainCamera)
        {
            _config = config;
            _mainCamera = mainCamera;
        }

        public IEnumerator Initialize()
        {
            _lightViews = new Dictionary<int, TorchLightView>();

            _renderer = new ScreenSpaceRender(new ScreenSpaceRenderConfig
            {
                mainCamera = _mainCamera,
                blurMatrix = _config.postMatrix,
                blurShader = _config.postShader,
                textureResolution = _config.renderTextureResolution,
                cameraCullingMask = 1 << LayerIndex.TorchLight,
                textureFilterMode = FilterMode.Point,
                cameraDepth = CameraRenderPriority.TorchCameraDepth,
                textureFormat = RenderTextureFormat.R16,
                renderName = "TorchLight"
            });

            GameObject rootGameObject = new ("TorchLight");
            _root = rootGameObject.transform;

            _torchSpriteMaterial = new Material(_config.torchLightShader);
            _warFogSpriteMaterial = new Material(_config.warFogSpriteShader);
            
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            
            yield break;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _renderer.BeginCameraRendering(camera);
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _renderer.EndCameraRendering(camera);

            Shader.SetGlobalTexture(ShaderPropertyId.TorchLightMapTexture, _renderer.lastRenderResult.texture);
            Shader.SetGlobalVector(ShaderPropertyId.TorchLightMapRotation, _renderer.lastRenderResult.textureRotationSinCosWorldSpace);
            Shader.SetGlobalVector(ShaderPropertyId.TorchLightMapOffset, _renderer.lastRenderResult.textureOffsetWorldSpace);
            Shader.SetGlobalVector(ShaderPropertyId.TorchLightMapWorldSize, _renderer.lastRenderResult.textureSizeWorldSpace);
        }

        public int CreatePointLight(TorchLightSetup lightSetup)
        {
            lightSetup.material = _torchSpriteMaterial;
            
            TorchLightView lightView = Object.Instantiate(_config.torchLightViewPrefab, _root);
            lightView.Initialize(lightSetup);
            
            _lightViews[_nextId] = lightView;

            return _nextId++;
        }

        public int CreateWarFogLight(TorchLightSetup lightSetup)
        {
            lightSetup.material = _warFogSpriteMaterial;
            
            TorchLightView lightView = Object.Instantiate(_config.torchLightViewPrefab, _root);
            lightView.Initialize(lightSetup);
            
            _lightViews[_nextId] = lightView;

            return _nextId++;
        }

        public void RemoveLight(int lightId)
        {
            if (!_lightViews.TryGetValue(lightId, out TorchLightView lightView))
            {
                throw new InvalidOperationException($"Torch light with id {lightId} was not found");
            }

            Object.Destroy(lightView.gameObject);

            _lightViews.Remove(lightId);
        }

        public void SetLightTransform(int lightId, Vector3 position, Vector3 direction)
        {
            if (!_lightViews.TryGetValue(lightId, out TorchLightView lightView))
            {
                throw new InvalidOperationException($"Torch light with id {lightId} was not found");
            }
            
            lightView.UpdatePositionRotation(position, Mathf.Atan2(direction.x, direction.z));
        }

        public void Update()
        {
            foreach ((int lightId, TorchLightView lightView) in _lightViews)
            {
                lightView.RebuildMesh();
            }
        }

        public void Dispose()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }
    }
}
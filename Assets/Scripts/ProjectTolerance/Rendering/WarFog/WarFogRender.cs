using System;
using System.Collections;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Layers;
using ProjectTolerance.Rendering.AmbientLight;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogRender : IDisposable
    {
        private const int TextureSizeMultiplier = 16;
        
        private readonly WarFogConfig _config;
        private readonly AmbientLightController _ambientLight;

        private RenderTexture _sunTexture;
        private RenderTexture _stateTexture;
        private RenderTexture _stateTextureSpreadAndBlur;
        private Vector2Int _playerPrevPosition;
        private Material _ambientMaterial;
        private Camera _warFogCamera;

        public WarFogRender (WarFogConfig config, 
                             AmbientLightController ambientLight)
        {
            _config = config;
            _ambientLight = ambientLight;
        }
        
        public IEnumerator Initialize()
        {
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            
            Texture2D wallTexture = _ambientLight.wallTexture;

            _sunTexture = new RenderTexture(wallTexture.width * TextureSizeMultiplier, wallTexture.height * TextureSizeMultiplier, 0, wallTexture.graphicsFormat)
            {
                name = "WarFogSun",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _stateTexture = new RenderTexture(wallTexture.width * TextureSizeMultiplier, wallTexture.height * TextureSizeMultiplier, 0, wallTexture.graphicsFormat)
            {
                name = "WarFogStateTextureRaw",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _ambientMaterial = new Material(_config.ambientShader);

            Shader.SetGlobalTexture(ShaderPropertyId.WarFogMapTexture, _sunTexture);

            _warFogCamera = new GameObject("WarFogCamera").AddComponent<Camera>();
            _warFogCamera.orthographic = true;
            _warFogCamera.clearFlags = CameraClearFlags.SolidColor;
            _warFogCamera.backgroundColor = Color.black;
            _warFogCamera.cullingMask = 1 << LayerIndex.WarFogLight;
            _warFogCamera.depth = CameraRenderPriority.WarFogCameraDepth;
            _warFogCamera.aspect = wallTexture.width / (float) wallTexture.height;
            _warFogCamera.orthographicSize = Mathf.Min(wallTexture.width, wallTexture.height) * 0.5f;
            _warFogCamera.targetTexture = _sunTexture;

            Vector3 center = _ambientLight.textureBounds.center;
            _warFogCamera.transform.position = new Vector3(center.x, 1, center.y);
            _warFogCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

            yield break;
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (_warFogCamera != camera)
            {
                return;
            }
            
            BoundsInt textureBounds = _ambientLight.textureBounds;

            RenderTexture tempTexture = RenderTexture.GetTemporary(
                textureBounds.size.x * TextureSizeMultiplier, 
                textureBounds.size.y * TextureSizeMultiplier, 
                0, 
                RenderTextureFormat.R16);
            
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.WallTexture, _ambientLight.wallTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.SunTexture, _sunTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.StateTexture, _stateTexture);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.Blur, _config.blur);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.LinearFall, _config.linearFall / TextureSizeMultiplier);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.BilinearFall, _config.bilinearFall);

            Graphics.Blit(null, tempTexture, _ambientMaterial);
            Graphics.Blit(tempTexture, _stateTexture);

            RenderTexture.ReleaseTemporary(tempTexture);
        }

        public void Dispose()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }
    }
}
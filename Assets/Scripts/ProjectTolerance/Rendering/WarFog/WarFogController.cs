using System;
using System.Collections;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Layers;
using ProjectTolerance.Player;
using ProjectTolerance.Rendering.AmbientLight;
using UnityEngine;
using UnityEngine.Rendering;

using Object = UnityEngine.Object;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogController : IDisposable
    {
        private readonly WarFogConfig _config;
        private readonly AmbientLightController _ambientLight;
        private readonly PlayerController _player;

        private RenderTexture _sunTexture;
        private RenderTexture _stateTexture;
        private Vector2Int _playerPrevPosition;
        private Material _ambientMaterial;
        private Camera _warFogCamera;
        private Transform _root;

        private WarFogPixelView _playerPixel;

        public WarFogController (WarFogConfig config, 
                                 AmbientLightController ambientLight, 
                                 PlayerController player)
        {
            _config = config;
            _ambientLight = ambientLight;
            _player = player;
        }
        
        public IEnumerator Initialize()
        {
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            
            Texture2D wallTexture = _ambientLight.wallTexture;

            _sunTexture = new RenderTexture(wallTexture.width, wallTexture.height, 0, RenderTextureFormat.R16)
            {
                name = "WarFog_Sun",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _stateTexture = new RenderTexture(wallTexture.width, wallTexture.height, 0, RenderTextureFormat.R16)
            {
                name = "WarFog_State",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            _root = new GameObject("WarFog").transform;

            _warFogCamera = new GameObject("WarFogCamera").AddComponent<Camera>();
            _warFogCamera.orthographic = true;
            _warFogCamera.backgroundColor = Color.black;
            _warFogCamera.clearFlags = CameraClearFlags.SolidColor;
            _warFogCamera.cullingMask = 1 << LayerId.WarFog;
            _warFogCamera.depth = CameraRenderPriority.WarFogCameraDepth;
            _warFogCamera.aspect = wallTexture.width / (float) wallTexture.height;
            _warFogCamera.orthographicSize = Mathf.Min(wallTexture.width, wallTexture.height) * 0.5f;
            _warFogCamera.targetTexture = _sunTexture;

            Vector3 center = _ambientLight.textureBounds.center;
            _warFogCamera.transform.position = new Vector3(center.x, 1, center.y);
            _warFogCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            _warFogCamera.transform.SetParent(_root);
            
            _playerPixel = Object.Instantiate(_config.warFogPixelViewPrefab, _root);

            _ambientMaterial = new Material(_config.ambientShader);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.WallTexture, wallTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.SunTexture, _sunTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.StateTexture, _stateTexture);

            Shader.SetGlobalTexture(ShaderPropertyId.WarFogMapTexture, _stateTexture);
            
            yield break;
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != _warFogCamera)
            {
                return;
            }
            
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.Blur, _config.blur);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.LinearFall, _config.linearFall);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.BilinearFall, _config.bilinearFall);
            
            BoundsInt textureBounds = _ambientLight.textureBounds;
            
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                textureBounds.size.x, 
                textureBounds.size.y, 
                0, 
                RenderTextureFormat.R16);
            
            Graphics.Blit(_stateTexture, tempTexture, _ambientMaterial);
            
            Graphics.Blit(tempTexture, _stateTexture);

            RenderTexture.ReleaseTemporary(tempTexture);
        }

        public void Update()
        {
            _playerPixel.transform.position = _player.position;

            // Vector3 offset = _player.position - new Vector3(textureBounds.position.x, 0, textureBounds.position.y);
            // offset.x = Mathf.Clamp(offset.x, 0, textureBounds.size.x - 1);
            // offset.y = Mathf.Clamp(offset.z, 0, textureBounds.size.y - 1);
            // Vector2Int playerPosition = new Vector2Int(Mathf.FloorToInt(offset.x), Mathf.FloorToInt(offset.y));
            //
            // if (playerPosition != _playerPrevPosition)
            // {
            //     _sunTexture.SetPixel(_playerPrevPosition.x, _playerPrevPosition.y, Color.black);
            //     _sunTexture.SetPixel(playerPosition.x, playerPosition.y, Color.white);
            //     _sunTexture.Apply();
            //
            //     _playerPrevPosition = playerPosition;
            // }
        }

        public void Dispose()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Layers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightController : IDisposable
    {
        private readonly TorchLightConfig _config;
        private readonly MainCameraController _mainCamera;
        
        private Transform _root;
        private Dictionary<int, TorchLightView> _lightViews;
        private int _nextId;
        private RenderTexture _torchTexture;
        private Camera _torchCamera;

        public TorchLightController (TorchLightConfig config, MainCameraController mainCamera)
        {
            _config = config;
            _mainCamera = mainCamera;
        }

        public IEnumerator Initialize()
        {
            _lightViews = new Dictionary<int, TorchLightView>();

            GameObject rootGameObject = new ("TorchLight");
            _root = rootGameObject.transform;

            GameObject torchCameraGameObject = new GameObject("TorchLightCamera");
            torchCameraGameObject.transform.SetParent(_root);
            _torchCamera = torchCameraGameObject.AddComponent<Camera>();
            _torchCamera.orthographic = true;
            _torchCamera.backgroundColor = Color.black;
            _torchCamera.clearFlags = CameraClearFlags.SolidColor;
            _torchCamera.cullingMask = 1 << LayerId.TorchLight;
            _torchCamera.depth = CameraRenderPriority.TorchCameraDepth;
            
            yield break;
        }

        public int CreateLight(float spreadAngleRad, float spreadRange)
        {
            TorchLightView lightView = Object.Instantiate(_config.torchLightViewPrefab, _root);
            lightView.Initialize(spreadAngleRad, spreadRange);
            
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

        public void UpdateLightData(int lightId, Vector3 position, float rotationRad)
        {
            if (!_lightViews.TryGetValue(lightId, out TorchLightView lightView))
            {
                throw new InvalidOperationException($"Torch light with id {lightId} was not found");
            }
            
            lightView.UpdatePositionRotation(position, rotationRad);
        }

        public void Update()
        {
            Vector3 viewNorth = Vector3.ProjectOnPlane(_mainCamera.forward, Vector3.up).normalized;
            Vector3 viewEast = Vector3.Cross(Vector3.up, viewNorth);
            float screenRatio = Screen.width / (float) Screen.height;
            Vector3 mainCameraRotationDeg = _mainCamera.rotationDegrees;
            Vector2 viewSize = ComputeViewSize(screenRatio, _mainCamera.orthographicSize, mainCameraRotationDeg.x);
            Vector3 cameraPosition = _mainCamera.position;

            Vector2Int requiredTextureSize = default;
            if (screenRatio > 0)
            {
                requiredTextureSize.x = Mathf.CeilToInt(_config.renderTextureResolution * screenRatio);
                requiredTextureSize.y = _config.renderTextureResolution;
            }
            else
            {
                requiredTextureSize.x = _config.renderTextureResolution;
                requiredTextureSize.y = Mathf.CeilToInt(_config.renderTextureResolution / screenRatio);
            }

            if (_torchTexture == null ||
                _torchTexture.width != requiredTextureSize.x ||
                _torchTexture.height != requiredTextureSize.y)
            {
                if (_torchTexture != null)
                {
                    _torchTexture.Release();
                    _torchTexture = null;
                }
                
                _torchTexture = RenderTexture.GetTemporary(requiredTextureSize.x, requiredTextureSize.y, 0, RenderTextureFormat.R8);
                _torchTexture.name = "TorchLight";
                _torchTexture.filterMode = FilterMode.Bilinear;
                
                _torchCamera.orthographicSize = Mathf.Min(viewSize.x, viewSize.y);
                _torchCamera.targetTexture = _torchTexture;
                _torchCamera.aspect = viewSize.x / viewSize.y;
            }

            foreach ((int lightId, TorchLightView lightView) in _lightViews)
            {
                lightView.RebuildMesh();
            }
            
            _torchCamera.transform.position = cameraPosition + Vector3.up;
            _torchCamera.transform.rotation = Quaternion.Euler(90, mainCameraRotationDeg.y, 0);

            float mapRotationRad = mainCameraRotationDeg.y * Mathf.Deg2Rad;
            Vector2 mapRotationSinCos = new (Mathf.Cos(mapRotationRad), Mathf.Sin(mapRotationRad));
            Vector3 bottomLeft = cameraPosition - viewNorth * viewSize.y - viewEast * viewSize.x;
            Vector3 bottomRight = cameraPosition - viewNorth * viewSize.y + viewEast * viewSize.x;
            Vector3 topRight = cameraPosition + viewNorth * viewSize.y + viewEast * viewSize.x;
            Vector3 topLeft = cameraPosition + viewNorth * viewSize.y - viewEast * viewSize.x;
            
            Debug.DrawLine(bottomLeft, topRight, Color.magenta);
            Debug.DrawLine(bottomRight, topLeft, Color.magenta);
            
            Shader.SetGlobalTexture(ShaderPropertyId.TorchLightMapTexture, _torchTexture);
            Shader.SetGlobalVector(ShaderPropertyId.TorchLightMapRotation, mapRotationSinCos);
            Shader.SetGlobalVector(ShaderPropertyId.TorchLightMapOffset, bottomLeft * -1);
            Shader.SetGlobalVector(ShaderPropertyId.TorchLightMapWorldSize, viewSize * 2);
        }

        private static Vector2 ComputeViewSize(float screenRatio, float orthographicSize, float pitchAngleDegrees)
        {
            return new Vector2
            {
                x = orthographicSize * screenRatio,
                y = orthographicSize / Mathf.Cos((90 - pitchAngleDegrees) * Mathf.Deg2Rad)
            };
        }

        public void Dispose()
        {
            if (_torchTexture != null)
            {
                _torchTexture.Release();
                _torchTexture = null;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

using Object = UnityEngine.Object;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightController : IUnorderedUpdatable
    {
        private readonly TorchLightConfig _config;
        private readonly CameraController _camera;

        private Transform _root;
        private Dictionary<int, TorchLightView> _lightViews;
        private int _nextId;

        public TorchLightController (TorchLightConfig config, CameraController camera)
        {
            _config = config;
            _camera = camera;
        }

        public IEnumerator Initialize()
        {
            _lightViews = new Dictionary<int, TorchLightView>();

            GameObject gameObject = new GameObject("TorchLights");
            _root = gameObject.transform;
            
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
            Vector3 viewNorth = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            Vector3 viewEast = Vector3.Cross(Vector3.up, viewNorth);
            Vector2 viewSize = ComputeViewSize(Screen.width / (float) Screen.height, _camera.orthographicSize, _camera.rotationDegrees.x);
            Vector3 cameraPosition = _camera.position;
            
            Vector3 topLeft = cameraPosition + viewNorth * viewSize.y - viewEast * viewSize.x;
            Vector3 topRight = cameraPosition + viewNorth * viewSize.y + viewEast * viewSize.x;
            Vector3 bottomRight = cameraPosition - viewNorth * viewSize.y + viewEast * viewSize.x;
            Vector3 bottomLeft = cameraPosition - viewNorth * viewSize.y - viewEast * viewSize.x;
            
            Debug.DrawLine(bottomLeft, topRight, Color.magenta);
            Debug.DrawLine(topLeft, bottomRight, Color.magenta);

            foreach ((int lightId, TorchLightView lightView) in _lightViews)
            {
                lightView.DebugDrawRaycasts();
                
                lightView.RebuildMesh();
                
                lightView.DebugDrawRaycasts();
            }
        }
        
        private static Vector2 ComputeViewSize(float screenRatio, float orthographicSize, float pitchAngleDegrees)
        {
            return new Vector2
            {
                x = orthographicSize * screenRatio,
                y = orthographicSize / Mathf.Cos((90 - pitchAngleDegrees) * Mathf.Deg2Rad)
            };
        }
    }
}
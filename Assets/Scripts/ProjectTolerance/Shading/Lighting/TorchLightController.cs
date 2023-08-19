using System;
using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.GameCycle;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightController : IUnorderedUpdatable
    {
        private const float SamplesPerRad = 1;
        
        private readonly TorchLightConfig _config;

        private Transform _root;
        private Dictionary<int, TorchLightView> _lightViews;
        private int _nextId;

        public TorchLightController (TorchLightConfig config)
        {
            _config = config;
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
            lightView.Initialize(spreadAngleRad, spreadRange,  Mathf.CeilToInt(spreadAngleRad * SamplesPerRad));
            
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
            foreach ((int lightId, TorchLightView lightView) in _lightViews)
            {
                lightView.DebugDrawRaycasts();
            }
        }
    }
}
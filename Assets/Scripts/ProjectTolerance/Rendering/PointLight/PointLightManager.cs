using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace ProjectTolerance.Rendering.PointLight
{
    public class PointLightManager
    {
        private readonly PointLightManagerConfig _managerConfig;
        
        private Transform _root;
        private Dictionary<int, PointLightView> _lightViews;
        private int _nextId;
        private Material _torchSpriteMaterial;
        private Material _warFogSpriteMaterial;

        public PointLightManager (PointLightManagerConfig managerConfig)
        {
            _managerConfig = managerConfig;
        }

        public IEnumerator Initialize()
        {
            _lightViews = new Dictionary<int, PointLightView>();

            _root = new GameObject("PointLightManager").transform;

            _torchSpriteMaterial = new Material(_managerConfig.pointLightShader);
            _warFogSpriteMaterial = new Material(_managerConfig.warFogLightShader);

            yield break;
        }

        public int CreatePointLight(PointLightSetup lightSetup)
        {
            lightSetup.material = _torchSpriteMaterial;
            
            PointLightView lightView = Object.Instantiate(_managerConfig._pointLightViewPrefab, _root);
            lightView.Initialize(lightSetup);
            
            _lightViews[_nextId] = lightView;

            return _nextId++;
        }

        public int CreateWarFogLight(PointLightSetup lightSetup)
        {
            lightSetup.material = _warFogSpriteMaterial;
            
            PointLightView lightView = Object.Instantiate(_managerConfig._pointLightViewPrefab, _root);
            lightView.Initialize(lightSetup);
            
            _lightViews[_nextId] = lightView;

            return _nextId++;
        }

        public void RemoveLight(int lightId)
        {
            if (!_lightViews.TryGetValue(lightId, out PointLightView lightView))
            {
                throw new InvalidOperationException($"Point light with id {lightId} was not found");
            }

            Object.Destroy(lightView.gameObject);

            _lightViews.Remove(lightId);
        }

        public void SetLightTransform(int lightId, Vector3 position, Vector3 direction)
        {
            if (!_lightViews.TryGetValue(lightId, out PointLightView lightView))
            {
                throw new InvalidOperationException($"Point light with id {lightId} was not found");
            }
            
            lightView.UpdatePositionRotation(position, Mathf.Atan2(direction.x, direction.z));
        }

        public void Update()
        {
            foreach ((int lightId, PointLightView lightView) in _lightViews)
            {
                lightView.RebuildMesh();
            }
        }
    }
}
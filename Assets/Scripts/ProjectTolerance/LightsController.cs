using System.Linq;
using UnityEngine;

namespace ProjectTolerance
{
    public class LightsController : MonoBehaviour
    {
        private struct LightData
        {
            public Light light;
            public float intensity;
        }
        
        [SerializeField] private Light[] _lightSources;
        
        private float _transitionTime;
        private LightData[] _lights;
        private float _power;
        private float _targetPower;

        private void Awake()
        {
            _power = 1;
            _targetPower = 1;
            _lights = _lightSources.Select(light => new LightData
            {
                intensity = light.intensity,
                light = light
            }).ToArray();
        }

        private void Update()
        {
            if (_targetPower > _power || _targetPower < _power)
            {
                _power = Mathf.MoveTowards(_power, _targetPower, Time.deltaTime / _transitionTime);
                
                foreach (var lightData in _lights)
                {
                    lightData.light.intensity = lightData.intensity * _power;
                }
            }
        }

        public void SwitchPower(float power, float transitionTime)
        {
            _targetPower = power;
            _transitionTime = transitionTime;
        }
    }
}
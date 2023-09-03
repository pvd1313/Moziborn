using System.Collections;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class WeatherController
    {
        private readonly WeatherConfig _config;
        
        private WeatherView _weatherView;

        public WeatherController (WeatherConfig config)
        {
            _config = config;
        }
        
        public IEnumerator Initialize()
        {
            _weatherView = Object.Instantiate(_config.weatherViewPrefab);
            
            yield break;
        }

        public void SetLightIntensity(float sunIntensity01, float ambientIntensity01)
        {
            _weatherView.sun.intensity = sunIntensity01;
            RenderSettings.ambientIntensity = ambientIntensity01;
        }
    }
}
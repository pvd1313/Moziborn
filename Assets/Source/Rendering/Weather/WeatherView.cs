using UnityEngine;

namespace ProjectTolerance.Rendering.Weather
{
    public class WeatherView : MonoBehaviour
    {
        public Light sun => _sun;
        
        [SerializeField] private Light _sun;
    }
}
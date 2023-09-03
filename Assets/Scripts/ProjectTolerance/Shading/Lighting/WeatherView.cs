using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class WeatherView : MonoBehaviour
    {
        public Light sun => _sun;
        
        [SerializeField] private Light _sun;
    }
}
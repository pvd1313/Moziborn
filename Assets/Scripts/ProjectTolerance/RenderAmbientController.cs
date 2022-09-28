using UnityEngine;

namespace ProjectTolerance
{
    public class RenderAmbientController : MonoBehaviour
    {
        private Color _ambientColor;
        private float _power;
        private float _targetPower;
        private float _transitionDuration;
        
        private void Awake()
        {
            _ambientColor = RenderSettings.ambientLight;
        }

        private void Update()
        {
            _power = Mathf.MoveTowards(_power, _targetPower, Time.deltaTime / _transitionDuration);
            RenderSettings.ambientLight = _ambientColor * _power;
        }

        public void SwitchPower(float power, float transitionDuration)
        {
            _targetPower = power;
            _transitionDuration = transitionDuration;
        }
    }
}
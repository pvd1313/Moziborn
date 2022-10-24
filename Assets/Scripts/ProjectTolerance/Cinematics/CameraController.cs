using System.Collections;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraController : IUnorderedUpdatable
    {
        public Vector3 CameraDirection => _cameraView.CameraDirection;
        
        private readonly CameraConfig _config;
        
        private CameraView _cameraView;
        private Vector3 _targetPosition; 

        
        public CameraController (CameraConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _cameraView = GameObject.Instantiate(_config.cameraViewPrefab);
            
            yield break;
        }

        public void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
        }

        public void Update()
        {
            _cameraView.OriginPosition = Vector3.Lerp(_cameraView.OriginPosition, _targetPosition, 4 * Time.deltaTime);
        }
    }
}
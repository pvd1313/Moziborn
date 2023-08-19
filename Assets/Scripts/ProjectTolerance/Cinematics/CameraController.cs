using System.Collections;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraController : IUnorderedLateUpdatable
    {
        public Vector3 forward => _cameraView.unityCamera.transform.forward;
        public Vector3 right => _cameraView.unityCamera.transform.right;
        
        private readonly CameraConfig _config;
        
        private CameraView _cameraView;
        private Vector3 _desiredPosition;
        private Vector3 _velocity;

        public CameraController (CameraConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _cameraView = Object.Instantiate(_config.viewPrefab);
            
            yield break;
        }

        public void SetDesiredPosition(Vector3 position)
        {
            _desiredPosition = position;

            _cameraView.unityCamera.ScreenPointToRay(Input.mousePosition);
        }

        public Ray ScreenPointToRay(Vector3 screenPosition)
        {
            return _cameraView.unityCamera.ScreenPointToRay(screenPosition);
        }
        
        public void LateUpdate()
        {
            _cameraView.transform.position = Vector3.SmoothDamp(_cameraView.transform.position, _desiredPosition, ref _velocity, _config.smoothTime);
        }
    }
}
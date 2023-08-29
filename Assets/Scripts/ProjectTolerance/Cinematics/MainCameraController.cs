using System.Collections;
using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class MainCameraController
    {
        public Vector3 forward => _cameraView.unityCamera.transform.forward;
        public Vector3 position => _cameraView.transform.position;
        public Vector3 rotationDegrees => _cameraView.transform.rotation.eulerAngles;
        public float orthographicSize => _cameraView.unityCamera.orthographicSize;

        private readonly CameraConfig _config;
        
        private CameraView _cameraView;
        private Vector3 _desiredPosition;
        private Vector3 _velocity;

        public MainCameraController (CameraConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _cameraView = Object.Instantiate(_config.viewPrefab);
            _cameraView.transform.rotation = Quaternion.Euler(_config.rotationAngleDegrees);
            _cameraView.unityCamera.orthographicSize = _config.orthographicSize;
            _cameraView.unityCamera.depth = CameraRenderPriority.MainCameraDepth;
            
            yield break;
        }

        public void AnimateMovePosition(Vector3 position)
        {
            _desiredPosition = position;
        }

        public Ray ScreenPointToRay(Vector3 screenPosition)
        {
            return _cameraView.unityCamera.ScreenPointToRay(screenPosition);
        }
        
        public void Update()
        {
            _cameraView.transform.position = Vector3.SmoothDamp(
                _cameraView.transform.position, 
                _desiredPosition, 
                ref _velocity, 
                _config.smoothTime,
                float.PositiveInfinity,
                Time.unscaledDeltaTime);
        }
    }
}
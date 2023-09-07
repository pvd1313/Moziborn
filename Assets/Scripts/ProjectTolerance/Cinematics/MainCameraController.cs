using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectTolerance.Cinematics
{
    public class MainCameraController
    {
        public Vector3 lookDirection => _cameraView.unityCamera.transform.forward;
        public Vector3 pivot => _cameraView.transform.position;
        public Vector3 rotationDeg => _cameraView.transform.rotation.eulerAngles;
        public float orthographicSize => _cameraView.unityCamera.orthographicSize;

        private readonly CameraConfig _config;
        
        private CameraView _cameraView;
        private Vector3 _desiredPosition;
        private Vector3 _velocity;
        private Vector3 _cameraPosition;

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
            _cameraPosition = Vector3.SmoothDamp(
                _cameraPosition, 
                _desiredPosition, 
                ref _velocity, 
                _config.smoothTime,
                float.PositiveInfinity,
                Time.deltaTime);

            _cameraView.transform.position = SnapToGrid(_cameraPosition);
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float texelSize0 = 2 * orthographicSize / Screen.height;
            float inclination = Mathf.Cos((90 - _config.rotationAngleDegrees.x) * Mathf.Deg2Rad);
            float texelSize1 = 2 * orthographicSize / inclination / Screen.height;
            
            Vector2 pos2D = new (position.z, position.x);
            pos2D = RotateVector(pos2D, _config.rotationAngleDegrees.y * -1 * Mathf.Deg2Rad);
            pos2D.x = Mathf.CeilToInt(pos2D.x / texelSize1) * texelSize1;
            pos2D.y = Mathf.CeilToInt(pos2D.y / texelSize0) * texelSize0;
            pos2D = RotateVector(pos2D, _config.rotationAngleDegrees.y * Mathf.Deg2Rad);
            
            float y = Mathf.CeilToInt(position.y / texelSize0) * texelSize0;
            
            return new Vector3(pos2D.y, y, pos2D.x);
        }
        
        private static Vector2 RotateVector(Vector2 direction, float angleRad)
        {
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            float x = cos * direction.x - sin * direction.y;
            float y = sin * direction.x + cos * direction.y;

            return new Vector2(x, y);
        }
    }
}
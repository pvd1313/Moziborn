using System.Collections;
using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraController
    {
        private readonly CameraConfig _config;
        private CameraView _cameraView;

        public Vector3 CameraPosition => _cameraView.CameraPosition;
        
        public CameraController (CameraConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _cameraView = GameObject.Instantiate(_config.cameraViewPrefab);
            
            yield break;
        }
    }
}
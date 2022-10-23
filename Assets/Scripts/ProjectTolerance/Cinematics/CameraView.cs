using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraView : MonoBehaviour
    {
        public Vector3 CameraPosition => _camera.transform.position;
        
        [SerializeField] private Camera _camera;
    }
}
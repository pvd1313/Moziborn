using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraView : MonoBehaviour
    {
        public Vector3 CameraPosition => _camera.transform.position;

        public Vector3 OriginPosition
        {
            get => transform.position;
            set => transform.position = value;
        }
        
        [SerializeField] private Camera _camera;
    }
}
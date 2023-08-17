using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraView : MonoBehaviour
    {
        public Vector3 forward => _camera.transform.forward;

        public Vector3 right => _camera.transform.right;

        public Vector3 OriginPosition
        {
            get => transform.position;
            set => transform.position = value;
        }
        
        [SerializeField] private Camera _camera;
    }
}
using UnityEngine;

namespace ProjectTolerance.Cinematics
{
    public class CameraView : MonoBehaviour
    {
        public Camera unityCamera => _unityCamera;

        [SerializeField] private Camera _unityCamera;
    }
}
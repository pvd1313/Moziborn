using UnityEngine;

namespace Moziborn.Cinematics
{
    public class CameraView : MonoBehaviour
    {
        public Camera unityCamera => _unityCamera;

        [SerializeField] private Camera _unityCamera;
    }
}
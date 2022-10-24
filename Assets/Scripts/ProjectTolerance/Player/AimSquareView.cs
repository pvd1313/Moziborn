using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectTolerance.Player
{
    public class AimSquareView : MonoBehaviour
    {
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        [SerializeField] private MeshRenderer _renderer;
        
        private void Start()
        {
            // _renderer.sharedMaterial.renderQueue = (int) RenderQueue.Transparent + 1;
        }
    }
}
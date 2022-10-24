using ProjectTolerance.Shading;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class AimSquareView : MonoBehaviour
    {
        private const float AnimationDuration = 0.2f;
        
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _blockColor;
        
        private bool _isVisible;
        private float _alpha;
        private bool _isBlock;
        private MaterialPropertyBlock _materialBlock;

        private void Start()
        {
            _materialBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            _alpha = Mathf.Clamp01(_alpha + Time.deltaTime / AnimationDuration * (_isVisible ? 1 : -1));

            var color = _isBlock ? _blockColor : _normalColor;
            _normalColor.a = _alpha;
            _materialBlock.SetColor(ShaderPropertyId.Color, color);
            _meshRenderer.SetPropertyBlock(_materialBlock);
        }
        
        public void SetVisible(bool isVisible)
        {
            _isVisible = isVisible;
        }

        public void SetBlock(bool isBlock)
        {
            _isBlock = isBlock;
        }
    }
}
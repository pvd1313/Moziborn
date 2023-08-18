using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerView : MonoBehaviour
    {
        public Collider viewBlockCollider => _viewBlockCollider;
        public Rigidbody body => _body;
        public Transform meshTransform => _meshTransform;

        [SerializeField] private Collider _viewBlockCollider;
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Transform _meshTransform;
    }
}
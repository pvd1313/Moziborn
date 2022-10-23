using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerView : MonoBehaviour
    {
        public Bounds ColliderBounds => _collider.bounds;

        [SerializeField] private Collider _collider;
        [SerializeField] private Rigidbody _rigidbody;

        public void Translate(Vector3 motion)
        {
            _rigidbody.MovePosition(_rigidbody.position + motion);
        }
    }
}
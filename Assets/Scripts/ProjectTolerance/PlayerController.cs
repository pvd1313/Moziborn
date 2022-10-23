using UnityEngine;

namespace ProjectTolerance
{
    public class PlayerController : MonoBehaviour
    {
        public Bounds Bounds => _collider.bounds;
        public float MovementSpeed => _movementSpeed;
        
        [SerializeField] private Transform _camera;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private Collider _collider;

        private void FixedUpdate()
        {
            var forward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;

            if (Input.GetKey(KeyCode.W))
            {
                forward *= 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                forward *= -1;
            }
            else
            {
                forward *= 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                right *= 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                right *= -1;
            }
            else
            {
                right *= 0;
            }

            var motion = (forward + right).normalized * _movementSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + motion);
        }
    }
}
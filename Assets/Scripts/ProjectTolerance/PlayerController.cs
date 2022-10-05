using UnityEngine;

namespace ProjectTolerance
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _camera;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _movementSpeed;

        private void FixedUpdate()
        {
            var forward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;

            var inputForward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            var inputRight = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;

            forward *= inputForward;
            right *= inputRight;

            var motion = (forward + right).normalized * _movementSpeed;
            // _rigidbody.MovePosition(_rigidbody.position + motion);
            
            _rigidbody.AddForce(motion, ForceMode.Acceleration);
        }
    }
}
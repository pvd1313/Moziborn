using UnityEngine;

namespace ProjectTolerance
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _camera;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _animationSpeed;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _maxSpeed;

        private void FixedUpdate()
        {
            var forwardVector = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            var rightVector = Vector3.Cross(Vector3.up, forwardVector).normalized;

            var forward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            var right = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;

            forwardVector *= forward;
            rightVector *= right;

            var accel = (forwardVector + rightVector).normalized * _acceleration;
            _rigidbody.AddForce(accel / Time.fixedDeltaTime, ForceMode.Acceleration);

            var velocity = _rigidbody.velocity;
            var localVel = transform.InverseTransformDirection(velocity.normalized);
            _animator.SetFloat("MotionSpeed", velocity.magnitude / _animationSpeed);

            if (velocity.magnitude > _maxSpeed)
            {
                _rigidbody.velocity = velocity.normalized * _maxSpeed;
            }

            if (localVel.magnitude > 0)
            {
                _animator.SetFloat("MotionForward",  localVel.z);
                _animator.SetFloat("MotionRight", localVel.x);
            }
            
        }
    }
}
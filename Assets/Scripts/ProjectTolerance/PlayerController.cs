using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _animationBakedSpeed;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _speedMultiplierMin;
        [SerializeField] private float _speedMultiplierMax;
        [SerializeField] private float _speedIncreasePerCycle;

        private Vector3 _currentDirection;
        private float _currentProgress;
        private bool _sleeping;
        private float _speedMultiplier;
        private float _awakingTimer;

        private void Awake()
        {
            _sleeping = true;
            _speedMultiplier = _speedMultiplierMin;
        }

        private void FixedUpdate()
        {
            if (!_sleeping)
            {
                _currentProgress += Time.fixedDeltaTime;
                AdvancePosition(Time.fixedDeltaTime);
            }
            
            var desiredForward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            var desiredRight = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;

            if (desiredForward != 0 || desiredRight != 0)
            {
                _awakingTimer += Time.deltaTime;

                if (_awakingTimer < 0.1f)
                {
                    desiredForward = 0;
                    desiredRight = 0;
                }
            }
            else
            {
                _awakingTimer = 0;
            }

            if (!_sleeping && (desiredForward != 0 || desiredRight != 0))
            {
                _speedMultiplier = Mathf.Min(_speedMultiplierMax, _speedMultiplier + _speedIncreasePerCycle * Time.fixedDeltaTime);
            }
            
            if (_currentProgress > 1 / _speedMultiplier || _sleeping)
            {
                _currentProgress %= 1 / _speedMultiplier;
                
                AdvancePosition(_currentProgress * -1);

                _currentDirection = new Vector3(desiredRight, 0, desiredForward).normalized;
                
                AdvancePosition(_currentProgress);
                
                _animator.SetFloat("MotionForward", _currentDirection.z);
                _animator.SetFloat("MotionRight", _currentDirection.x);

                if (desiredForward == 0 && desiredRight == 0)
                {
                    _currentProgress = 0;
                    _sleeping = true;
                    _speedMultiplier = _speedMultiplierMin;
                }
                else
                {
                    _sleeping = false;
                }
            }
            
            _animator.SetFloat("MotionProgress", _currentProgress * _speedMultiplier);
        }

        private void AdvancePosition(float deltaTime)
        {
            _rigidbody.MovePosition(_rigidbody.position + _rigidbody.rotation * _currentDirection * _animationBakedSpeed * deltaTime * _speedMultiplier);
        }


        // [SerializeField] private Transform _camera;
        // [SerializeField] private Rigidbody _rigidbody;
        // [SerializeField] private float _animationSpeed;
        // [SerializeField] private Animator _animator;
        // [SerializeField] private float _acceleration;
        // [SerializeField] private float _maxSpeed;
        //
        // private void FixedUpdate()
        // {
        //     var forwardVector = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
        //     var rightVector = Vector3.Cross(Vector3.up, forwardVector).normalized;
        //
        //     var forward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        //     var right = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        //
        //     forwardVector *= forward;
        //     rightVector *= right;
        //
        //     var accel = (forwardVector + rightVector).normalized * _acceleration;
        //     _rigidbody.AddForce(accel / Time.fixedDeltaTime, ForceMode.Acceleration);
        //
        //     var velocity = _rigidbody.velocity;
        //     var localVel = transform.InverseTransformDirection(velocity.normalized);
        //     _animator.SetFloat("MotionSpeed", velocity.magnitude / _animationSpeed);
        //
        //     if (velocity.magnitude > _maxSpeed)
        //     {
        //         _rigidbody.velocity = velocity.normalized * _maxSpeed;
        //     }
        //
        //     if (localVel.magnitude > 0)
        //     {
        //         _animator.SetFloat("MotionForward",  localVel.z);
        //         _animator.SetFloat("MotionRight", localVel.x);
        //     
        // }
    }
}
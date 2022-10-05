using UnityEngine;

namespace ProjectTolerance
{
    public class CubeBotController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _thrust;

        private void FixedUpdate()
        {
            var forwardMotion = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            
            _rigidbody.AddForce(_rigidbody.rotation * Vector3.forward * forwardMotion * _thrust, ForceMode.Force);
        }
    }
}
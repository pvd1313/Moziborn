using UnityEngine;

namespace Moziborn.Buildings
{
    public class DoorView : MonoBehaviour
    {
        private static readonly int IsOpenId = Animator.StringToHash("IsOpen");

        /// <summary>
        /// 1 = closed, 0 = open.
        /// </summary>
        public float doorState { get ; private set; }

        [SerializeField] private float _switchDuration;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider[] _colliders;

        private float _targetValue;
        private bool _collidersEnabled;

        private void Awake()
        {
            _targetValue = 1;
            _collidersEnabled = true;
        }

        private void Update()
        {
            doorState = Mathf.MoveTowards(doorState, _targetValue, Time.deltaTime / _switchDuration);
            
            _animator.SetFloat(IsOpenId, doorState);

            bool enableColliders = doorState > 0;
            if (enableColliders != _collidersEnabled)
            {
                _collidersEnabled = enableColliders;
                foreach (Collider col in _colliders)
                {
                    col.enabled = _collidersEnabled;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            _targetValue = 0;
        }

        private void OnTriggerExit(Collider other)
        {
            _targetValue = 1;
        }
    }
}
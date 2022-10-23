using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class CubeBotLeg : MonoBehaviour
    {
        [SerializeField] private Transform _legTransform;
        [SerializeField] private float _legSyncDuration;
        [SerializeField] private float _legLiftHeight;
        [SerializeField] private float _legLiftDistance;
        [SerializeField] private float _legSleepDuration;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _timeOffset;

        private Vector3 _legOriginalLocalPosition;
        private Vector3 _legCurrentWorldPosition;
        private Vector3 _legGoalWorldPosition;
        private float _timer;
        private bool _sleeping;

        private void Awake()
        {
            _legOriginalLocalPosition = transform.InverseTransformPoint(_legTransform.position);
            _legCurrentWorldPosition = _legTransform.position;

            _timer += (_legSleepDuration + _legSyncDuration) * _timeOffset;
        }

        private void FixedUpdate()
        {
            _timer += Time.fixedDeltaTime;
            
            if (_sleeping)
            {
                _legTransform.position = _legCurrentWorldPosition;
                
                if (_timer >= _legSleepDuration)
                {
                    _timer -= _legSleepDuration;
                    _sleeping = false;
                    _legGoalWorldPosition = _rigidbody.transform.position + _rigidbody.transform.rotation * _legOriginalLocalPosition + _rigidbody.velocity * (_legSyncDuration + _legSleepDuration) * 0.75f;
                }
            }
            else
            {
                var distance = Vector3.Distance(_legCurrentWorldPosition, _legGoalWorldPosition);
                //if (distance > _legLiftDistance)
                {
                    var resultPos = Vector3.Lerp(_legCurrentWorldPosition, _legGoalWorldPosition, _timer / _legSyncDuration);
                    resultPos.y += Mathf.Sin(_timer / _legSyncDuration * Mathf.PI) * _legLiftHeight;
                    _legTransform.position = resultPos;
                }

                if (_timer > _legSyncDuration)
                {
                    //if (distance > _legLiftDistance)
                    {
                        _legCurrentWorldPosition = _legGoalWorldPosition;
                    }
                    
                    _timer -= _legSyncDuration;
                    _sleeping = true;
                }
            }
        }
    }
}
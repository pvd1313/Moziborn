using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class IKLegsController : MonoBehaviour
    {
        [Serializable]
        public struct LegConfig
        {
            public Transform transform;
            public float timeOffset;
        }
        
        private class LegAnimation
        {
            public Vector3 bakedPosition;
            public Vector3 currentPosition;
            public Vector3 goalPosition;
            public float timeOffset;
            public Transform transform;
        }
        
        [SerializeField] private LegConfig[] _legs;
        [SerializeField] private float _liftHeightMin;
        [SerializeField] private float _liftHeightMax;
        [SerializeField] private float _syncDuration;
        [SerializeField] private float _sleepDuration;
        [SerializeField] private Rigidbody _rigidbody;

        private LegAnimation[] _animations;

        private void Awake()
        {
            _animations = new LegAnimation[_legs.Length];
            for (var i = 0; i < _legs.Length; i++)
            {
                _animations[i] = new LegAnimation
                {
                    timeOffset = _legs[i].timeOffset,
                    transform = _legs[i].transform,
                    bakedPosition = transform.InverseTransformPoint(_legs[i].transform.position),
                };
            }
        }

        private void LateUpdate()
        {
            var cycleDuration = _sleepDuration + _syncDuration;

            foreach (var legAnimation in _animations)
            {
                var time = (Time.time + cycleDuration * legAnimation.timeOffset) % cycleDuration;
                if (time > _sleepDuration)
                {
                    legAnimation.goalPosition = _rigidbody.transform.position 
                                                + _rigidbody.transform.rotation * legAnimation.bakedPosition 
                                                + _rigidbody.velocity * _sleepDuration;
                    
                    var progress = (time - _sleepDuration) / _syncDuration;
                    var resultPos = Vector3.Lerp(legAnimation.currentPosition, legAnimation.goalPosition, progress);
                    var liftDelta = _liftHeightMax - _liftHeightMin;
                    resultPos.y += Mathf.Sin(progress * Mathf.PI) * (liftDelta * _rigidbody.velocity.magnitude + _liftHeightMin);

                    legAnimation.transform.position = resultPos;
                }
                else
                {
                    legAnimation.currentPosition = legAnimation.goalPosition;
                    legAnimation.transform.position = legAnimation.goalPosition;
                }
            }
        }
    }
}
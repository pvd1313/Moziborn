using System;
using TMPro;
using UnityEngine;

namespace ProjectTolerance
{
    public class DoorController : MonoBehaviour
    {
        private static readonly int IsOpenId = Animator.StringToHash("IsOpen");
        
        [SerializeField] private float _switchDuration;
        [SerializeField] private Animator _doorAnimator;
        
        private float _progress;
        private float _targetValue;

        private void Update()
        {
            _progress = Mathf.MoveTowards(_progress, _targetValue, Time.deltaTime / _switchDuration);
            _doorAnimator.SetFloat(IsOpenId, _progress);
        }

        private void OnTriggerEnter(Collider other)
        {
            _targetValue = 1;
        }

        private void OnTriggerExit(Collider other)
        {
            _targetValue = 0;
        }
    }
}
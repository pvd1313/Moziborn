using System;
using Moziborn.Animation;
using UnityEngine;

namespace Moziborn.Buildings
{
    public class DoorView
    {
        public Vector3 position => _instance.transform.position;
        
        /// <summary>
        /// 1 = closed, 0 = open.
        /// </summary>
        public float doorState { get; private set; }

        public event Action<DoorView, string> animatorEvent;
        
        private readonly DoorPrefab _instance;
        private bool _collidersEnabled;
        private bool _isPlayingAnimation;
        private float _targetState;

        public DoorView(DoorPrefab instance)
        {
            _instance = instance;
            _collidersEnabled = true;
            doorState = 1;
            _instance.animator.SetFloat(AnimatorPropertyId.isOpen, 1);
        }

        public void Update()
        {
            if (_isPlayingAnimation)
            {
                doorState = Mathf.MoveTowards(
                    doorState, 
                    _targetState, 
                    Time.deltaTime / _instance.switchDuration);
                
                _instance.animator.SetFloat(AnimatorPropertyId.isOpen, doorState);
                
                bool enableColliders = doorState > 0;
                if (enableColliders != _collidersEnabled)
                {
                    _collidersEnabled = enableColliders;
                    foreach (Collider collider in _instance.colliders)
                    {
                        collider.enabled = _collidersEnabled;
                    }
                }

                if (doorState == _targetState)
                {
                    _isPlayingAnimation = false;
                }
            }
            else
            {
                _targetState = _instance.colliderTriggerHandler.CheckTriggerActive() ? 0 : 1;
                if (_targetState == doorState)
                {
                    return;
                }

                _isPlayingAnimation = true;
                
                if (doorState == 1 && _targetState == 0)
                {
                    animatorEvent?.Invoke(this, AnimatorEventId.doorBeginOpening);
                }

                if (doorState == 0 && _targetState == 1)
                {
                    animatorEvent?.Invoke(this, AnimatorEventId.doorBeginClosing);
                }
            }
        }
    }
}
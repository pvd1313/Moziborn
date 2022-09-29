using UnityEngine;

namespace ProjectTolerance
{
    public class DoorController : MonoBehaviour
    {
        private static readonly int IsOpenId = Animator.StringToHash("IsOpen");
        
        [SerializeField] private float _alwaysOpenRadius;
        [SerializeField] private Vector3 _doorCenter;
        [SerializeField] private float _switchDuration;
        [SerializeField] private Collider[] _proximityTriggers;
        [SerializeField] private Animator _doorAnimator;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private AmbientLightMapController _ambientLightMapController;
        [SerializeField] private Vector2Int[] _occupiedPixels;
        
        private float _progress;
        private float _targetValue;

          private void Update()
        {
            _targetValue = 0;
            
            var playerBounds = _playerController.Bounds;
            var playerCenter = playerBounds.center;
            var playerDistance = Vector3.Distance(playerCenter, transform.TransformPoint(_doorCenter));
            
            if (CheckAnyTriggerIntersectsBounds(playerBounds))
            {
                var arrivalTime = playerDistance / _playerController.MovementSpeed;
                if (arrivalTime < _switchDuration + 0.1f)
                {
                    _targetValue = 1;
                }
            }
            
            if (playerDistance < _alwaysOpenRadius)
            {
                _targetValue = 1;
            }
            
            if (_targetValue > _progress || _targetValue < _progress)
            {
                _progress = Mathf.MoveTowards(_progress, _targetValue, Time.deltaTime / _switchDuration);
                _doorAnimator.SetFloat(IsOpenId, _progress);
                foreach (var occupiedPixel in _occupiedPixels)
                {
                    _ambientLightMapController.ModifyPixel(occupiedPixel.x, occupiedPixel.y, 0.75f * _progress);    
                }
            }
        }

          public bool IsClosingOrClosed()
        {
            return _targetValue == 0;
        }

        private bool CheckAnyTriggerIntersectsBounds(Bounds bounds)
        {
            foreach (var trigger in _proximityTriggers)
            {
                if (trigger.bounds.Intersects(bounds))
                {
                    return true;
                }
            }

            return false;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(_doorCenter), _alwaysOpenRadius);
        }
    }
}
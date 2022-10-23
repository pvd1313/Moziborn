using System.Collections;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerController : IUnorderedFixedUpdatable
    {
        private readonly PlayerConfig _config;

        private PlayerView _playerView;

        public Bounds ViewBounds => _playerView.ColliderBounds;

        public PlayerController (PlayerConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _playerView = GameObject.Instantiate(_config.playerViewPrefab);
            
            yield break;
        }

        public void FixedUpdate()
        {
            var forward = (Vector3.forward + Vector3.right).normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            
            if (Input.GetKey(KeyCode.W))
            {
                forward *= 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                forward *= -1;
            }
            else
            {
                forward *= 0;
            }
        
            if (Input.GetKey(KeyCode.D))
            {
                right *= 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                right *= -1;
            }
            else
            {
                right *= 0;
            }
        
            var motion = (forward + right).normalized * _config.movementSpeed * Time.fixedDeltaTime;
            _playerView.Translate(motion);
        }
    }
}
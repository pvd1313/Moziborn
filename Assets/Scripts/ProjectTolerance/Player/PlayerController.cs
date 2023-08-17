using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerController : IUnorderedFixedUpdatable
    {
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly CameraController _camera;
        private readonly BuildingController _buildingController;

        private PlayerView _playerView;

        public PlayerController (PlayerConfig config, 
                                 PlayerInputController input, 
                                 CameraController camera,
                                 BuildingController buildingController)
        {
            _config = config;
            _input = input;
            _camera = camera;
            _buildingController = buildingController;
        }

        public IEnumerator Initialize()
        {
            _playerView = Object.Instantiate(_config.playerViewPrefab);

            yield break;
        }

        public void FixedUpdate()
        {
            PlayerInputState input = _input.State;

            if (input.isMoving)
            {
                Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
                Vector3 cameraRight = Vector3.ProjectOnPlane(_camera.right, Vector3.up).normalized;
                Vector3 movement = cameraForward * input.forward + cameraRight * input.right;
                
                _playerView.Position += movement.normalized * Time.fixedDeltaTime * _config.playerSpeed;

                _buildingController.OnPlayerBoundsUpdate(_playerView.ColliderBounds);
            }

            _camera.SetTargetPosition(_playerView.Position);
        }
    }
}
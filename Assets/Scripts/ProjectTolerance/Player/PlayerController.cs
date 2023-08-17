using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerController : IUnorderedFixedUpdatable, IUnorderedUpdatable
    {
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly CameraController _camera;
        private readonly BuildingController _buildingController;

        private PlayerView _playerView;
        private Vector3 _playerMeshPosition;
        private Vector3 _playerMeshVelocity;

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
            PlayerInputState input = _input.state;

            if (input.isMoving)
            {
                Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
                Vector3 cameraRight = Vector3.ProjectOnPlane(_camera.right, Vector3.up).normalized;
                Vector3 movement = cameraForward * input.forward + cameraRight * input.right;

                Vector3 targetPosition = _playerView.rigidbody.position + movement.normalized * Time.fixedDeltaTime * _config.playerSpeed;
                _playerView.rigidbody.MovePosition(targetPosition);
            }

            _camera.SetDesiredPosition(_playerView.rigidbody.position);
        }

        public void Update()
        {
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition, 
                _playerView.rigidbody.position, 
                ref _playerMeshVelocity, 
                _config.meshSmoothTime);
            
            _playerView.meshTransform.position = _playerMeshPosition;
            
            _buildingController.OnPlayerBoundsUpdate(_playerView.viewBlockCollider.bounds);
        }
    }
}
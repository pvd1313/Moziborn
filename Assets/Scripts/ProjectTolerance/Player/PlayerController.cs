using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using ProjectTolerance.Shading.Lighting;
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
        private TorchLightView _torchLightView;
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
            _torchLightView = Object.Instantiate(_config.torchLightViewPrefab, _playerView.transform);
            _torchLightView.transform.localPosition = new Vector3(0, 1, 0);

            _torchLightView.Initialize(_config.torchLightAngle, _config.torchLightDistance, 10);

            yield break;
        }

        public void FixedUpdate()
        {
            PlayerInputState input = _input.state;

            if (input.isMoving)
            {
                Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
                Vector3 cameraRight = Vector3.ProjectOnPlane(_camera.right, Vector3.up).normalized;
                Vector3 movement = cameraForward * input.motionForward + cameraRight * input.motionRight;

                Vector3 targetPosition = _playerView.body.position + movement.normalized * Time.fixedDeltaTime * _config.playerSpeed;
                _playerView.body.MovePosition(targetPosition);
            }

            _camera.SetDesiredPosition(_playerView.body.position);
        }

        public void Update()
        {
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition, 
                _playerView.body.position, 
                ref _playerMeshVelocity, 
                _config.meshSmoothTime);
            
            _playerView.meshTransform.position = _playerMeshPosition;
            _torchLightView.transform.position = _playerMeshPosition + new Vector3(0, 1, 0);
            
            _buildingController.OnPlayerBoundsUpdate(_playerView.viewBlockCollider.bounds);
            
            _torchLightView.DebugDrawRaycasts(_input.state.viewAngleRad);
        }
    }
}
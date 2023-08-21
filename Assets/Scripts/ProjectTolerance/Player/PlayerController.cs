using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Shading.Lighting;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerController
    {
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly MainCameraController _mainCamera;
        private readonly BuildingController _buildingController;
        private readonly TorchLightController _torchLightController;

        private PlayerView _playerView;
        private Vector3 _playerMeshPosition;
        private Vector3 _playerMeshVelocity;
        private int _torchLightId;

        public PlayerController (PlayerConfig config, 
                                 PlayerInputController input, 
                                 MainCameraController mainCamera,
                                 BuildingController buildingController,
                                 TorchLightController torchLightController)
        {
            _config = config;
            _input = input;
            _mainCamera = mainCamera;
            _buildingController = buildingController;
            _torchLightController = torchLightController;
        }

        public IEnumerator Initialize()
        {
            _playerView = Object.Instantiate(_config.playerViewPrefab);
            
            _torchLightId = _torchLightController.CreateLight(_config.torchLightAngleDeg * Mathf.Deg2Rad, _config.torchLightDistance);

            yield break;
        }

        public void Update()
        {
            PlayerInputState input = _input.state;

            if (input.isMoving)
            {
                Vector3 cameraForward = Vector3.ProjectOnPlane(_mainCamera.forward, Vector3.up).normalized;
                Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized;
                Vector3 movement = cameraForward * input.motionForward + cameraRight * input.motionRight;

                Vector3 targetPosition = _playerView.body.position + movement.normalized * Time.deltaTime * _config.playerSpeed;
                _playerView.body.MovePosition(targetPosition);
            }

            _mainCamera.AnimateMovePosition(_playerView.body.position);
            
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition, 
                _playerView.body.position, 
                ref _playerMeshVelocity, 
                _config.meshSmoothTime);
            
            _playerView.meshTransform.position = _playerMeshPosition;
            
            _torchLightController.UpdateLightData(_torchLightId, _playerMeshPosition, _input.state.aimDirectionAngleRad);

            _buildingController.OnPlayerBoundsUpdate(_playerView.viewBlockCollider.bounds);
        }
    }
}
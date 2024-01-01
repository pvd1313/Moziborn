using System.Collections;
using System.Collections.Generic;
using Moziborn.Buildings;
using Moziborn.Cinematics;
using Moziborn.Layers;
using Moziborn.Rendering.PointLight;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Moziborn.Player
{
    public class PlayerController
    {
        public Vector3 rigidbodyPosition => _playerView.body.position;
        
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly MainCameraController _mainCamera;
        private readonly BuildingController _buildingController;
        private readonly PointLightManager _pointLightManager;

        private PlayerView _playerView;
        private int _torchLightId;
        private Vector3 _playerMeshPosition;
        private Vector3 _playerMeshVelocity;
        private float _playerAngleVelocity;

        private List<int> _tempLights;

        public PlayerController (PlayerConfig config, 
                                 PlayerInputController input, 
                                 MainCameraController mainCamera,
                                 BuildingController buildingController,
                                 PointLightManager pointLightManager)
        {
            _config = config;
            _input = input;
            _mainCamera = mainCamera;
            _buildingController = buildingController;
            _pointLightManager = pointLightManager;
        }

        public IEnumerator Initialize()
        {
            _playerView = Object.Instantiate(_config.playerViewPrefab);

            _tempLights = new List<int>();

            _torchLightId = _pointLightManager.CreateLight(new PointLightSetup
            {
                layerIndex = LayerIndex.PointLight,
                range = _config.flashlightRange,
                spreadAngleRad = _config.flashlightAngleDeg * Mathf.Deg2Rad
            });

            yield break;
        }

        public void FixedUpdate()
        {
            PlayerInputState input = _input.state;

            if (input.mode == PlayerInputMode.Movement)
            {
                _playerView.body.velocity = input.movementDirection * _config.playerSpeed;
            }
            else
            {
                _playerView.body.velocity = Vector3.zero;
            }
        }

        public void Update()
        {
            Vector3 playerAimDirection = _input.state.aimDirection;
            
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition,
                _playerView.body.position,
                ref _playerMeshVelocity,
                _config.positionSmoothTime);

            if (_input.state.isDropFlareKeyPressed)
            {
                int lightId = _pointLightManager.CreateLight(new PointLightSetup
                {
                    layerIndex = LayerIndex.PointLight,
                    range = _config.flashlightRange,
                    spreadAngleRad = _config.flashlightAngleDeg * Mathf.Deg2Rad
                });

                _pointLightManager.SetLightTransform(lightId, _playerMeshPosition, _input.state.aimDirection);

                _tempLights.Add(lightId);
            }

            if (_input.state.isRemoveAllFlaresKeyPressed)
            {
                foreach (int lightId in _tempLights)
                {
                    _pointLightManager.RemoveLight(lightId);
                }
                
                _tempLights.Clear();
            }
            
            _playerView.meshTransform.position = _playerMeshPosition;

            _mainCamera.AnimateMovePosition(_playerView.body.position);

            _pointLightManager.SetLightTransform(_torchLightId, _playerMeshPosition, _input.state.aimDirection);
            
            _buildingController.UpdateViewerState(_playerView.viewBlockCollider.bounds, playerAimDirection);
        }

        private Vector3 InputToWorldDirection(Vector2 inputDirection)
        {
            Vector3 cameraForward = Vector3.ProjectOnPlane(_mainCamera.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);

            return cameraForward * inputDirection.y + cameraRight * inputDirection.x;
        }
    }
}
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
        private int _warFogLightId;
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

            Vector3 targetDirection = InputToWorldDirection(input.mode == PlayerInputMode.Movement ? input.movementDirection : input.aimDirection);

            float currentAngle = _playerView.body.rotation.eulerAngles.y;
            float targetAngle = Quaternion.LookRotation(targetDirection).eulerAngles.y;
            float resultAngle = Mathf.SmoothDampAngle(
                currentAngle, 
                targetAngle, 
                ref _playerAngleVelocity, 
                _config.rotationSmoothTime, 
                float.PositiveInfinity, 
                Time.fixedDeltaTime);
            
            _playerView.body.rotation = Quaternion.AngleAxis(resultAngle, Vector3.up);

            if (input.mode == PlayerInputMode.Movement)
            {
                float dot = Mathf.Max(0, Vector3.Dot(_playerView.body.rotation * Vector3.forward, targetDirection));
                _playerView.body.velocity = targetDirection * _config.playerSpeed * dot;
            }
            else
            {
                _playerView.body.velocity = Vector3.zero;
            }
        }

        public void Update()
        {
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition,
                _playerView.body.position,
                ref _playerMeshVelocity,
                _config.positionSmoothTime);

            Vector3 playerDirectionWorld = _playerView.body.rotation * Vector3.forward;
            Vector3 aimDirectionWorld = InputToWorldDirection(_input.state.aimDirection);

            if (_input.state.isDropFlareKeyPressed)
            {
                int lightId = _pointLightManager.CreateLight(new PointLightSetup
                {
                    layerIndex = LayerIndex.PointLight,
                    range = _config.flashlightRange,
                    spreadAngleRad = _config.flashlightAngleDeg * Mathf.Deg2Rad
                });

                _pointLightManager.SetLightTransform(lightId, _playerMeshPosition, playerDirectionWorld);

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

            _pointLightManager.SetLightTransform(_torchLightId, _playerMeshPosition, playerDirectionWorld);

            _pointLightManager.SetLightTransform(_warFogLightId, _playerMeshPosition, playerDirectionWorld);
            
            _buildingController.UpdateViewerState(_playerView.viewBlockCollider.bounds, aimDirectionWorld);
        }

        private Vector3 InputToWorldDirection(Vector2 inputDirection)
        {
            Vector3 cameraForward = Vector3.ProjectOnPlane(_mainCamera.lookDirection, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);

            return cameraForward * inputDirection.y + cameraRight * inputDirection.x;
        }
    }
}
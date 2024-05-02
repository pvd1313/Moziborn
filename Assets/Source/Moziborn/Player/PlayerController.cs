using System;
using System.Collections;
using System.Collections.Generic;
using Moziborn.Buildings;
using Moziborn.Characters;
using Moziborn.Cinematics;
using Moziborn.Layers;
using Moziborn.Rendering.PointLight;
using UnityEngine;

namespace Moziborn.Player
{
    public class PlayerController
    {
        public Vector3 rigidbodyPosition => _characterView.rigidbody.position;
        
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly MainCameraController _mainCamera;
        private readonly BuildingController _buildingController;
        private readonly PointLightManager _pointLightManager;
        private readonly CharacterFactory _characterFactory;

        private CharacterView _characterView;
        private int _torchLightId;
        private Vector3 _playerMeshPosition;
        private Vector3 _playerMeshVelocity;
        private float _playerAngleVelocity;

        private List<int> _tempLights;

        public PlayerController (PlayerConfig config, 
                                 PlayerInputController input, 
                                 MainCameraController mainCamera,
                                 BuildingController buildingController,
                                 PointLightManager pointLightManager,
                                 CharacterFactory characterFactory)
        {
            _config = config;
            _input = input;
            _mainCamera = mainCamera;
            _buildingController = buildingController;
            _pointLightManager = pointLightManager;
            _characterFactory = characterFactory;
        }

        public IEnumerator Initialize()
        {
            _characterView = _characterFactory.CreateView(_config.playerMeshPrefab);

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
                _characterView.animator.SetFloat("WalkSpeed", _characterView.rigidbody.velocity.magnitude / _config.movementSpeed);
                _characterView.rigidbody.velocity = input.movementDirection * _config.movementSpeed;
            }
            else
            {
                _characterView.rigidbody.velocity = Vector3.zero;
                _characterView.animator.SetFloat("WalkSpeed", 0);
            }
        }

        public void Update()
        {
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition,
                _characterView.rigidbody.position,
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
            
            _characterView.meshRoot.position = _playerMeshPosition;
            
            Quaternion aimRotation = Quaternion.LookRotation(_input.state.aimDirection);
            float aimDeltaAngle = Quaternion.Angle(aimRotation, _characterView.meshRoot.rotation);
            float aimDeltaTime = aimDeltaAngle / _config.rotationSpeed;
            float speedMultiplier = Mathf.Clamp01(Mathf.Pow(aimDeltaTime / _config.rotationSmoothTime, 1f));
            _characterView.meshRoot.rotation = Quaternion.RotateTowards(
                    _characterView.meshRoot.rotation, 
                    aimRotation,
                    Time.deltaTime * _config.rotationSpeed * speedMultiplier);

            _mainCamera.AnimateMovePosition(_characterView.rigidbody.position);

            _pointLightManager.SetLightTransform(_torchLightId, _playerMeshPosition, _characterView.meshRoot.forward);
            
            _buildingController.UpdateViewerState(_characterView.viewBlockCollider.bounds, _input.state.aimDirection);
        }
    }
}
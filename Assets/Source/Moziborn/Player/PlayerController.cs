using System.Collections;
using System.Collections.Generic;
using Moziborn.Buildings;
using Moziborn.Characters;
using Moziborn.Cinematics;
using Moziborn.Effects;
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
        private AimLineView _gunAimLineView;
        
        private Vector3 _playerMeshPosition;
        private Quaternion _playerMeshRotation;
        
        private Vector3 _playerMeshVelocity;
        private float _playerAngleVelocity;
        private float _gunCharge01;
        private float _aimLineTransparency01;

        private int _torchLightId;

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

            AimLinePrefab aimLineInstance = GameObject.Instantiate(_config.aimLineViewPrefab, _characterView.meshRoot);
            _gunAimLineView = new AimLineView(aimLineInstance);
            _gunAimLineView.SetLength(32);

            yield break;
        }

        public void FixedUpdate()
        {
            PlayerInputState input = _input.state;

            if (input.mode == PlayerInputMode.Movement)
            {
                _characterView.animator.SetFloat(
                    "WalkSpeed", 
                    _characterView.rigidbody.velocity.magnitude / _config.movementSpeed);
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
            PositionRotationUpdate();
            
            FlashLightUpdate();
            
            GunUpdate();
            
            _mainCamera.AnimateMovePosition(_characterView.rigidbody.position);
            
            _buildingController.UpdateViewerState(_characterView.viewBlockCollider.bounds, _input.state.aimDirection);
        }

        private void PositionRotationUpdate()
        {
            _playerMeshPosition = Vector3.SmoothDamp(
                _playerMeshPosition,
                _characterView.rigidbody.position,
                ref _playerMeshVelocity,
                _config.positionSmoothTime);
            
            Quaternion aimRotation = Quaternion.LookRotation(_input.state.aimDirection);
            float aimDeltaAngle = Quaternion.Angle(aimRotation, _characterView.meshRoot.rotation);
            float aimDeltaTime = aimDeltaAngle / _config.rotationSpeed;
            float speedMultiplier = Mathf.Clamp01(Mathf.Pow(aimDeltaTime / _config.rotationSmoothTime, 1f));
            _playerMeshRotation = Quaternion.RotateTowards(
                _characterView.meshRoot.rotation, 
                aimRotation,
                Time.deltaTime * _config.rotationSpeed * speedMultiplier);
            
            _characterView.meshRoot.position = _playerMeshPosition;
            _characterView.meshRoot.rotation = _playerMeshRotation;
        }

        private void FlashLightUpdate()
        {
            _pointLightManager.SetLightTransform(
                _torchLightId, 
                _playerMeshPosition, 
                _playerMeshRotation * Vector3.forward);
            
            if (_input.state.isDropFlareKeyPressed)
            {
                int lightId = _pointLightManager.CreateLight(new PointLightSetup
                {
                    layerIndex = LayerIndex.PointLight,
                    range = _config.flashlightRange,
                    spreadAngleRad = _config.flashlightAngleDeg * Mathf.Deg2Rad
                });

                _pointLightManager.SetLightTransform(
                    lightId, 
                    _playerMeshPosition, 
                    _playerMeshRotation * Vector3.forward);

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
        }

        private void GunUpdate()
        {
            if (_input.state.mode == PlayerInputMode.Fire)
            {
                if (_input.state.fireButtonState == InputButtonState.IsHeldDown)
                {
                    _gunCharge01 += Mathf.Clamp01(Time.deltaTime / _config.gunChargeDelay);
                }
                else
                {
                    _gunCharge01 = 0;
                }

                const float aimLineDistanceMax = 32;
                
                Ray ray = new (_playerMeshPosition + Vector3.up, _playerMeshRotation * Vector3.forward);
                
                if (Physics.Raycast(ray, 
                        out RaycastHit hit, 
                        aimLineDistanceMax, 
                        1 << LayerIndex.LightObstacle,
                        QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                    _gunAimLineView.SetLength(hit.distance);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * aimLineDistanceMax, Color.green);
                    _gunAimLineView.SetLength(aimLineDistanceMax);
                }

                _aimLineTransparency01 += Time.deltaTime / _config.aimLineFadeDelay;
                _gunAimLineView.SetColorBlend(_gunCharge01);
            }
            else
            {
                _aimLineTransparency01 -= Time.deltaTime / _config.aimLineFadeDelay;
                _gunCharge01 = 0;
            }

            _aimLineTransparency01 = Mathf.Clamp01(_aimLineTransparency01);
            _gunAimLineView.SetTransparency(_aimLineTransparency01);
        }
    }
}
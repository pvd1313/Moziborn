using System.Collections;
using System.Collections.Generic;
using Moziborn.Animation;
using Moziborn.Audio;
using Moziborn.Buildings;
using Moziborn.Characters;
using Moziborn.Cinematics;
using Moziborn.Layers;
using Moziborn.Rendering.PointLight;
using Moziborn.VisualEffects;
using UnityEngine;


namespace Moziborn.Player
{
    public class PlayerController
    {
        public Vector3 rigidbodyPosition => _characterView.rigidbodyPosition;
        
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly MainCameraController _mainCamera;
        private readonly BuildingController _buildingController;
        private readonly PointLightManager _pointLightManager;
        private readonly CharacterFactory _characterFactory;
        private readonly AudioController _audioController;

        private CharacterView _characterView;
        private AimLineView _gunAimLineView;
        
        private float _gunCharge01;
        private float _aimLineTransparency01;

        private int _torchLightId;
        private bool _torchLightActive;

        private List<int> _tempLights;

        public PlayerController (PlayerConfig config, 
                                 PlayerInputController input, 
                                 MainCameraController mainCamera,
                                 BuildingController buildingController,
                                 PointLightManager pointLightManager,
                                 CharacterFactory characterFactory,
                                 AudioController audioController)
        {
            _config = config;
            _input = input;
            _mainCamera = mainCamera;
            _buildingController = buildingController;
            _pointLightManager = pointLightManager;
            _characterFactory = characterFactory;
            _audioController = audioController;
        }

        public IEnumerator Initialize()
        {
            _characterView = _characterFactory.CreateView(CharacterMeshId.robot);
            _characterView.animatorEvent += OnAnimatorEvent;

            _tempLights = new List<int>();

            _torchLightId = _pointLightManager.CreateLight(new PointLightSetup
            {
                layerIndex = LayerIndex.PointLight,
                range = _config.flashlightRange,
                spreadAngleRad = _config.flashlightAngleDeg * Mathf.Deg2Rad
            });

            _torchLightActive = false;
            _pointLightManager.SetLightEnabled(_torchLightId, false);

            AimLinePrefab aimLineInstance = GameObject.Instantiate(_config.aimLineViewPrefab);
            _gunAimLineView = new AimLineView(aimLineInstance);
            _gunAimLineView.SetLength(32);
            _characterView.AttachTransformToMeshRoot(aimLineInstance.transform);

            _audioController.SetListener(_characterView);

            yield break;
        }

        public void FixedUpdate()
        {
            PlayerInputState input = _input.state;

            if (input.mode == PlayerInputMode.Movement)
            {
                _characterView.AnimatorSetFloat(
                    AnimatorPropertyId.walkSpeed, 
                    _characterView.rigidbodyVelocity.magnitude / _config.movementSpeed);
                
                _characterView.rigidbodyVelocity = input.movementDirection * _config.movementSpeed;
            }
            else
            {
                _characterView.rigidbodyVelocity = Vector3.zero;
                _characterView.AnimatorSetFloat(AnimatorPropertyId.walkSpeed, 0);
            }
        }

        public void Update()
        {
            _characterView.InterpolateMeshPosition(_config.positionSmoothTime);
            
            _characterView.RotateMeshTowards(
                _input.state.aimDirection,
                _config.rotationSpeed, 
                _config.rotationSmoothTime);
            
            FlashLightUpdate();
            
            GunUpdate();
            
            _mainCamera.AnimateMovePosition(_characterView.rigidbodyPosition);
            
            _buildingController.UpdateViewerState(_characterView.viewBlockColliderBounds, _input.state.aimDirection);
        }

        private void OnAnimatorEvent(string eventId)
        {
            if (eventId == AnimatorEventId.footStep)
            {
                _audioController.StartEvent(AudioEventId.footStep, _characterView);
            }
        }

        private void FlashLightUpdate()
        {
            _pointLightManager.SetLightTransform(
                _torchLightId, 
                _characterView.meshPosition, 
                _characterView.meshRotation * Vector3.forward);
            
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
                    _characterView.meshPosition, 
                    _characterView.meshRotation * Vector3.forward);

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

            if ((_input.state.flashLightButtonState.HasFlag(InputButtonState.PressedThisFrame)))
            {
                _torchLightActive = !_torchLightActive;
                _pointLightManager.SetLightEnabled(_torchLightId, _torchLightActive);
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
                
                Ray ray = new (
                    _characterView.meshPosition + Vector3.up, 
                    _characterView.meshRotation * Vector3.forward);
                
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
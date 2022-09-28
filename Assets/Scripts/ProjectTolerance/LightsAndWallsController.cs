using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class LightsAndWallsController : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private BuildingController _buildingController;
        [SerializeField] private DoorController _doorController;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _wallCutDuration;
        [SerializeField] private float _lightSwitchDuration;
        [SerializeField] private LightsController _interiorLights;
        [SerializeField] private LightsController _exteriorLights;
        [SerializeField] private LightsController _fakeLights;
        [SerializeField] private RenderAmbientController _ambientController;

        private void Update()
        {
            var playerPosition = _playerTransform.position + Vector3.up;
            
            _interiorLights.SwitchPower(1, _lightSwitchDuration);
            _exteriorLights.SwitchPower(1, _lightSwitchDuration);
            _fakeLights.SwitchPower(1, _lightSwitchDuration);
            _ambientController.SwitchPower(1, _lightSwitchDuration);
            
            if (_buildingController.RoomBounds.Contains(playerPosition))
            {
                if (_doorController.IsClosingOrClosed())
                {
                    _exteriorLights.SwitchPower(0, _lightSwitchDuration);
                    _ambientController.SwitchPower(0, _lightSwitchDuration);
                    _fakeLights.SwitchPower(0, _lightSwitchDuration);
                }
            }
            else
            {
                if (_doorController.IsClosingOrClosed())
                {
                    _interiorLights.SwitchPower(0, _lightSwitchDuration);
                    _fakeLights.SwitchPower(0, _lightSwitchDuration);
                }
            }
            
            if (_buildingController.RoomBounds.Contains(playerPosition))
            {
                _buildingController.SwitchVisibleParts(EBuildingPartLocation.North | EBuildingPartLocation.East, _wallCutDuration);
                _buildingController.SetExteriorDoorsInteriorExterior(0);
                return;
            }
            
            if (_doorController.IsClosed())
            {
                _buildingController.SetInteriorBulbsVisible(false);
            }
            else
            {
                _buildingController.SetInteriorBulbsVisible(true);
            }

            var ray = new Ray( _cameraTransform.position, _playerTransform.position - _cameraTransform.position);
            if (_buildingController.BuildingBounds.IntersectRay(ray))
            {
                _buildingController.SwitchVisibleParts(EBuildingPartLocation.None, _wallCutDuration);
                _buildingController.SetExteriorDoorsInteriorExterior(1);
                return;
            }
            
            _buildingController.SwitchVisibleParts(EBuildingPartLocation.All, _wallCutDuration);
            _buildingController.SetExteriorDoorsInteriorExterior(1);
        }
    }
}
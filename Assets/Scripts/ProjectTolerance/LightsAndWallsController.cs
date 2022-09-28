using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class LightsAndWallsController : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private BuildingController _buildingController;
        [SerializeField] private  DoorController _doorController;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _transitionDuration;
        [SerializeField] private LightsController _interiorLights;
        [SerializeField] private LightsController _exteriorLights;
        [SerializeField] private LightsController _fakeLights;

        private void Update()
        {
            var playerPosition = _playerTransform.position + Vector3.up;
            
            _interiorLights.SwitchPower(1, _transitionDuration * 2);
            _exteriorLights.SwitchPower(1, _transitionDuration * 2);
            _fakeLights.SwitchPower(1, _transitionDuration * 2);
            
            if (_buildingController.RoomBounds.Contains(playerPosition))
            {
                if (_doorController.IsClosingOrClosed())
                {
                    _exteriorLights.SwitchPower(0, _transitionDuration * 2);
                    _fakeLights.SwitchPower(0, _transitionDuration * 2);
                }
            }
            else
            {
                if (_doorController.IsClosingOrClosed())
                {
                    _interiorLights.SwitchPower(0, _transitionDuration * 2);
                    _fakeLights.SwitchPower(0, _transitionDuration * 2);
                }
            }
            
            if (_buildingController.RoomBounds.Contains(playerPosition))
            {
                _buildingController.SwitchVisibleParts(EBuildingPartType.NorthWall | EBuildingPartType.EastWall, _transitionDuration);
                _doorController.SetDoorsLayer(LayerNames.Interior);
                return;
            }

            var ray = new Ray( _cameraTransform.position, _playerTransform.position - _cameraTransform.position);
            if (_buildingController.BuildingBounds.IntersectRay(ray))
            {
                _buildingController.SwitchVisibleParts(EBuildingPartType.None, _transitionDuration);
                _doorController.SetDoorsLayer(LayerNames.Door);
                return;
            }
            
            _buildingController.SwitchVisibleParts(EBuildingPartType.All, _transitionDuration);
            _doorController.SetDoorsLayer(LayerNames.Door);
        }
    }
}